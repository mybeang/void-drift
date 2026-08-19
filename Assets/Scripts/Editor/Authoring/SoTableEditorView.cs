using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VD.Editor
{
    /// <summary>
    /// 재사용 베이스 (M2-3b) — 특정 <typeparamref name="T"/>(ScriptableObject)의 에셋들을 한 패널에서
    /// 목록(<see cref="MultiColumnListView"/>) + 상세 편집(<see cref="PropertyField"/>) + 신규/삭제로 다루는 제네릭 오서링 뷰.
    /// <para><b>EditorWindow가 아니라 VisualElement</b>라, 도메인 창(별창)이든 허브 창의 탭이든 동일 패널을 꽂아 재사용한다
    /// (확장성의 실체 — 관심사 분리: 도메인마다 뷰 하나, 한 창에 섞지 않음).</para>
    /// 도메인 에디터(예: Enemy)는 이 뷰를 상속해 <see cref="ConfigureColumns"/>(컬럼)·<see cref="AssetFolder"/>/<see cref="NewAssetBaseName"/>만 지정.
    /// 상세 편집은 SerializedObject를 PropertyField로 나열해 자동 바인딩(편집 즉시 SO 반영). 저장 정책은 M2-3e에서 확정.
    /// </summary>
    public class SoTableEditorView<T> : VisualElement where T : ScriptableObject
    {
        readonly List<T> _items = new List<T>();
        readonly MultiColumnListView _table;
        readonly VisualElement _detail;
        T _current;   // 현재 편집 중인 에셋(선택 전환·창 닫힘 시 디스크 저장 대상)

        /// <summary>신규 에셋 생성 폴더. 도메인이 override 가능.</summary>
        protected virtual string AssetFolder => "Assets/ScriptableObjects/Data";

        /// <summary>신규 에셋 기본 이름(확장자 제외). 도메인이 override 가능.</summary>
        protected virtual string NewAssetBaseName => "New" + typeof(T).Name;

        /// <summary>현재 로드된 에셋 목록(도메인 컬럼 bindCell에서 인덱스로 접근).</summary>
        protected IReadOnlyList<T> Items => _items;

        public SoTableEditorView()
        {
            style.flexGrow = 1;
            AddToClassList("so-table-editor");

            // 툴바: 신규 / 삭제 / 새로고침
            var toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(CreateNew) { text = "New" });
            toolbar.Add(new ToolbarButton(DeleteSelected) { text = "Delete" });
            toolbar.Add(new ToolbarButton(Reload) { text = "Reload" });
            Add(toolbar);

            // 좌: 목록(테이블) | 우: 상세
            var split = new TwoPaneSplitView(0, 280f, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            Add(split);

            _table = new MultiColumnListView { selectionType = SelectionType.Single, fixedItemHeight = 22f };
            ConfigureColumns(_table);
            _table.itemsSource = _items;
            _table.selectedIndicesChanged += OnSelectionChanged;
            split.Add(_table);

            _detail = new ScrollView();
            _detail.AddToClassList("so-detail");
            split.Add(_detail);

            RegisterCallback<DetachFromPanelEvent>(_ => SaveCurrent());   // 창 닫힘/제거 시 현재 편집 저장
            Reload();
        }

        /// <summary>컬럼 구성 — 베이스는 이름 컬럼만. 도메인이 override해 컬럼 추가(M2-3d).</summary>
        protected virtual void ConfigureColumns(MultiColumnListView table)
        {
            table.columns.Add(new Column
            {
                name = "name",
                title = "Name",
                stretchable = true,
                makeCell = () => new Label(),
                bindCell = (e, i) => ((Label)e).text = _items[i] != null ? _items[i].name : "<none>",
            });
        }

        /// <summary>목록 셀만 재바인딩(값·경고 표시 등이 바뀌었을 때). 데이터 재로드는 <see cref="Reload"/>.</summary>
        protected void RefreshRows() => _table.RefreshItems();

        /// <summary>프로젝트 내 모든 T 에셋을 로드해 목록 갱신.</summary>
        public void Reload()
        {
            SaveCurrent();
            _current = null;
            _items.Clear();
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) _items.Add(asset);
            }
            _table.RefreshItems();
            _detail.Clear();
        }

        void OnSelectionChanged(IEnumerable<int> _)
        {
            SaveCurrent();   // 이전 선택 편집을 디스크에 확정
            int i = _table.selectedIndex;
            _current = (i >= 0 && i < _items.Count) ? _items[i] : null;
            BuildDetail(_current);
        }

        /// <summary>현재 편집 중인 에셋을 dirty 표시 후 디스크 저장(변경 없으면 no-op). 파괴/삭제된 참조는 Unity의 fake-null로 안전 스킵.</summary>
        void SaveCurrent()
        {
            if (_current == null) return;
            EditorUtility.SetDirty(_current);
            AssetDatabase.SaveAssetIfDirty(_current);
        }

        /// <summary>
        /// 상세 패널 = 이름(에셋명, 편집 가능) + 선택 에셋의 SerializedObject를 PropertyField로 나열(자동 바인딩).
        /// 중첩(struct 등) 폴드아웃은 기본 펼침(UX). 도메인이 override하거나 <see cref="CustomizeDetail"/> 훅으로 확장.
        /// </summary>
        protected virtual void BuildDetail(T item)
        {
            _detail.Clear();
            if (item == null) return;

            // 에셋 파일명 편집 — 최상단(비주얼 등 직렬화 필드보다 위). 포커스 아웃/Enter 시 리네임.
            var nameField = new TextField("Name") { value = item.name };
            nameField.AddToClassList("so-name-field");
            nameField.RegisterCallback<FocusOutEvent>(_ => CommitRename(nameField, item));
            nameField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) CommitRename(nameField, item);
            });
            _detail.Add(nameField);

            var so = new SerializedObject(item);
            var it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyPath == "m_Script") continue;
                if (it.hasVisibleChildren) it.isExpanded = true;   // 중첩(스탯 등) 폴드아웃 기본 펼침
                _detail.Add(new PropertyField(it.Copy()));
            }
            _detail.Bind(so);

            CustomizeDetail(item, so, _detail);   // 도메인 확장 훅(예: 공격AI별 필드 처리)
        }

        /// <summary>상세 패널 도메인 확장 훅 — 기본 no-op. <paramref name="detail"/>=상세 컨테이너(도메인이 자식 필드 접근용). (예: 선택 AI에 따른 필드 비활성)</summary>
        protected virtual void CustomizeDetail(T item, SerializedObject so, VisualElement detail) { }

        /// <summary>에셋 파일명 변경(리네임). 성공 시 목록 갱신, 실패/무효 시 필드 원복.</summary>
        void CommitRename(TextField field, T item)
        {
            string newName = field.value?.Trim();
            if (item == null || string.IsNullOrEmpty(newName) || newName == item.name)
            {
                field.SetValueWithoutNotify(item != null ? item.name : string.Empty);
                return;
            }
            string path = AssetDatabase.GetAssetPath(item);
            string err = AssetDatabase.RenameAsset(path, newName);
            if (!string.IsNullOrEmpty(err))
            {
                Debug.LogWarning($"[SoTableEditor] 이름 변경 실패: {err}");
                field.SetValueWithoutNotify(item.name);
                return;
            }
            AssetDatabase.SaveAssets();
            _table.RefreshItems();                       // Name 컬럼 갱신(에셋 객체는 동일, 이름만 변경)
            field.SetValueWithoutNotify(item.name);
        }

        /// <summary>신규 에셋을 <see cref="AssetFolder"/>에 만들고 목록 갱신 후 선택.</summary>
        public void CreateNew()
        {
            if (!AssetDatabase.IsValidFolder(AssetFolder))
            {
                Debug.LogWarning($"[SoTableEditor] 대상 폴더 없음: {AssetFolder}");
                return;
            }
            var asset = ScriptableObject.CreateInstance<T>();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{AssetFolder}/{NewAssetBaseName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Reload();
            int idx = _items.IndexOf(asset);
            if (idx >= 0) _table.SetSelection(idx);
        }

        void DeleteSelected()
        {
            int i = _table.selectedIndex;
            if (i < 0 || i >= _items.Count) return;
            var asset = _items[i];
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) return;
            if (!EditorUtility.DisplayDialog("삭제 확인", $"'{asset.name}' 에셋을 삭제할까요?", "삭제", "취소")) return;
            if (_current == asset) _current = null;   // 저장 대상에서 제외(삭제될 것)
            AssetDatabase.DeleteAsset(path);
            Reload();
        }
    }
}
