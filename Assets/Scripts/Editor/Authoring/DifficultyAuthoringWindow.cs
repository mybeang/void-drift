using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VD.Core;

namespace VD.Editor
{
    /// <summary>
    /// 웨이브 난이도 오서링 창(밸런싱 패스 3-1·3-2). 단일 <see cref="WaveDifficultyConfig"/>를 편집하고,
    /// 그 데이터로 <b>계산된 per-wave 값(저/고HP·데미지4·동시상한·주기)</b>을 우측 프리뷰 테이블로 실시간 표시한다.
    /// (구 페이즈 테이블 툴 폐기 — 밸런스 수치를 코드가 아닌 데이터로 두는 것이 목적.)
    /// </summary>
    public sealed class DifficultyAuthoringWindow : EditorWindow
    {
        const string ConfigDir = "Assets/ScriptableObjects/Data";
        const string ConfigPath = ConfigDir + "/WaveDifficultyConfig.asset";

        WaveDifficultyConfig _config;
        VisualElement _inspectorHost;
        Button _createBtn;
        MultiColumnListView _preview;
        readonly List<int> _rows = new List<int>();

        [MenuItem("Window/Void Drift/Difficulty Authoring")]
        public static void ShowWindow()
        {
            var win = GetWindow<DifficultyAuthoringWindow>();
            win.titleContent = new GUIContent("Difficulty Authoring");
            win.minSize = new Vector2(680f, 420f);
        }

        void CreateGUI()
        {
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Authoring/SoTableEditor.uss");
            if (uss != null) rootVisualElement.styleSheets.Add(uss);

            _config = LoadOrFindConfig();

            var root = rootVisualElement;
            root.style.paddingLeft = 6; root.style.paddingRight = 6; root.style.paddingTop = 6; root.style.paddingBottom = 6;

            var title = new Label("Wave Difficulty Config");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 14; title.style.marginBottom = 4;
            root.Add(title);

            var objField = new ObjectField("Config") { objectType = typeof(WaveDifficultyConfig), value = _config, allowSceneObjects = false };
            objField.RegisterValueChangedCallback(e => { _config = e.newValue as WaveDifficultyConfig; RebuildInspector(); RefreshPreview(); });
            root.Add(objField);

            _createBtn = new Button(CreateConfig) { text = "Create Config Asset" };
            _createBtn.style.display = _config == null ? DisplayStyle.Flex : DisplayStyle.None;
            root.Add(_createBtn);

            var split = new TwoPaneSplitView(0, 320f, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            root.Add(split);

            var left = new ScrollView();
            _inspectorHost = new VisualElement();
            left.Add(_inspectorHost);
            split.Add(left);

            var right = new VisualElement(); right.style.flexGrow = 1; right.style.paddingLeft = 4;
            var pvTitle = new Label("Per-Wave Preview (계산값)");
            pvTitle.style.unityFontStyleAndWeight = FontStyle.Bold; pvTitle.style.marginBottom = 2;
            right.Add(pvTitle);
            right.Add(new Button(RefreshPreview) { text = "↻ 새로고침" });
            _preview = BuildPreviewTable();
            _preview.style.flexGrow = 1;
            right.Add(_preview);
            split.Add(right);

            RebuildInspector();
            RefreshPreview();
            // 필드 편집 → 프리뷰 자동 반영(주기적).
            root.schedule.Execute(RefreshPreviewCells).Every(500);
        }

        void RebuildInspector()
        {
            _inspectorHost.Clear();
            if (_createBtn != null) _createBtn.style.display = _config == null ? DisplayStyle.Flex : DisplayStyle.None;
            if (_config == null)
            {
                _inspectorHost.Add(new HelpBox("Config 에셋이 없습니다. 위에서 지정하거나 생성하세요.", HelpBoxMessageType.Info));
                return;
            }
            _inspectorHost.Add(new InspectorElement(_config));
        }

        MultiColumnListView BuildPreviewTable()
        {
            var t = new MultiColumnListView();
            AddCol(t, "wave", "Wave", w => w.ToString(), 48);
            AddStat(t, "low", "저HP", WaveStatKind.LowHp);
            AddStat(t, "high", "고HP", WaveStatKind.HighHp);
            AddStat(t, "single", "단발", WaveStatKind.DmgSingle);
            AddStat(t, "barrage", "탄막", WaveStatKind.DmgBarrage);
            AddStat(t, "charge", "돌진", WaveStatKind.DmgCharge);
            AddStat(t, "suicide", "자폭", WaveStatKind.DmgSuicide);
            AddCol(t, "cap", "상한", w => _config != null ? _config.CapAt(w).ToString() : "-", 48);
            AddCol(t, "intv", "주기", w => _config != null ? _config.IntervalAt(w).ToString("0.#") + "s" : "-", 52);
            t.itemsSource = _rows;
            return t;
        }

        void AddCol(MultiColumnListView t, string name, string title, Func<int, string> get, float w)
        {
            t.columns.Add(new Column
            {
                name = name, title = title, width = w,
                makeCell = () => new Label(),
                bindCell = (e, i) => ((Label)e).text = (i >= 0 && i < _rows.Count) ? get(_rows[i]) : string.Empty,
            });
        }

        void AddStat(MultiColumnListView t, string name, string title, WaveStatKind kind)
        {
            AddCol(t, name, title, w =>
            {
                if (_config == null) return "-";
                float elapsed = (w - 1f) * _config.secondsPerWave;   // 프리뷰: 해당 웨이브 시작 경과
                return _config.TryStatValue(kind, w, elapsed, out var v) ? v.ToString("0.#") : "—";
            }, 62);
        }

        void RefreshPreview()
        {
            _rows.Clear();
            int max = _config != null ? Mathf.Max(_config.scaledWaveMax + 3, 12) : 12;
            for (int w = 1; w <= max; w++) _rows.Add(w);
            if (_preview != null) { _preview.itemsSource = _rows; _preview.RefreshItems(); }
        }

        // 값만 갱신(행 수 변화 없을 때) — 필드 편집 실시간 반영용.
        void RefreshPreviewCells()
        {
            if (_config == null || _preview == null) return;
            int max = Mathf.Max(_config.scaledWaveMax + 3, 12);
            if (max != _rows.Count) RefreshPreview();
            else _preview.RefreshItems();
        }

        static WaveDifficultyConfig LoadOrFindConfig()
        {
            var c = AssetDatabase.LoadAssetAtPath<WaveDifficultyConfig>(ConfigPath);
            if (c != null) return c;
            foreach (var g in AssetDatabase.FindAssets("t:WaveDifficultyConfig"))
                return AssetDatabase.LoadAssetAtPath<WaveDifficultyConfig>(AssetDatabase.GUIDToAssetPath(g));
            return null;
        }

        void CreateConfig()
        {
            if (!AssetDatabase.IsValidFolder(ConfigDir))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Data");
            var c = CreateInstance<WaveDifficultyConfig>();
            AssetDatabase.CreateAsset(c, ConfigPath);
            AssetDatabase.SaveAssets();
            _config = c;
            RebuildInspector();
            RefreshPreview();
        }
    }
}
