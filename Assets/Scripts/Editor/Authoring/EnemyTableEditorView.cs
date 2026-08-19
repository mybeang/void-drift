using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VD.Core;

namespace VD.Editor
{
    /// <summary>
    /// 적 도메인 테이블 에디터 (M2-3c/d, M2-4b/c) — 재사용 베이스 <see cref="SoTableEditorView{T}"/>에 <see cref="EnemyDefinition"/> 바인딩.
    /// 컬럼(⚠·archetype·moveAI·attackAI·RangeLabel) + 상세 확장:
    /// <list type="bullet">
    /// <item><b>공격AI별 스탯 필드 비활성</b>(선택 <see cref="AttackAIType"/>에 안 쓰이는 stats 필드 그레이아웃).</item>
    /// <item><b>유효성 경고</b>(<see cref="EnemyValidation"/> R1·R2) — 경고 박스 + <b>모순 필드 red 테두리</b> + <b>목록 행 ⚠</b>, 실시간.</item>
    /// </list>
    /// R3(비주얼 라벨 교차, §6)는 M2-4d.
    /// </summary>
    public sealed class EnemyTableEditorView : SoTableEditorView<EnemyDefinition>
    {
        static readonly string[] AttackStatPaths =
            { "stats.fireInterval", "stats.projectileSpeed", "stats.barrageCount", "stats.suicideRadius" };

        // 값 변경 시 경고를 재판정할 필드(R1·R2 입력).
        static readonly string[] ValidationFields = { "moveAI", "attackAI", "archetype", "visual" };

        protected override string NewAssetBaseName => "Enemy_New";

        protected override void ConfigureColumns(MultiColumnListView table)
        {
            AddWarnColumn(table);           // ⚠ (경고 있는 행)
            base.ConfigureColumns(table);   // Name 컬럼
            AddTextColumn(table, "archetype", "Archetype", d => d.archetype.ToString());
            AddTextColumn(table, "move", "MoveAI", d => d.moveAI.ToString());
            AddTextColumn(table, "attack", "AttackAI", d => d.attackAI.ToString());
            AddTextColumn(table, "range", "Range", d => d.RangeLabel);
        }

        /// <summary>경고 표시 컬럼(좁게) — 경고 있는 행에 ⚠.</summary>
        void AddWarnColumn(MultiColumnListView table)
        {
            table.columns.Add(new Column
            {
                name = "warn",
                title = string.Empty,
                width = 22,
                makeCell = () => { var l = new Label(); l.AddToClassList("so-row-warn"); return l; },
                bindCell = (e, i) => ((Label)e).text =
                    (Items[i] != null && ValidateAll(Items[i]).Count > 0) ? "⚠" : string.Empty,
            });
        }

        /// <summary>읽기 전용 텍스트 컬럼 헬퍼 — 셀 = Label, 값 = <paramref name="get"/>(해당 행 에셋).</summary>
        void AddTextColumn(MultiColumnListView table, string name, string title, Func<EnemyDefinition, string> get)
        {
            table.columns.Add(new Column
            {
                name = name,
                title = title,
                width = 96,
                makeCell = () => new Label(),
                bindCell = (e, i) => ((Label)e).text = Items[i] != null ? get(Items[i]) : string.Empty,
            });
        }

        protected override void CustomizeDetail(EnemyDefinition item, SerializedObject so, VisualElement detail)
        {
            // 경고 박스(상단)
            var warnBox = new VisualElement();
            warnBox.AddToClassList("so-warning-box");
            detail.Insert(0, warnBox);
            RefreshValidation(detail, warnBox, item);

            // 공격AI별 스탯 필드 비활성(③) — 중첩 필드 비동기 빌드라 준비될 때까지 재시도
            ApplyAttackFieldStates(detail, so, 8);

            // moveAI/attackAI/archetype 변경 → 경고·하이라이트·행 재판정 (attackAI는 필드 비활성도 갱신)
            foreach (var path in ValidationFields)
            {
                var pf = detail.Query<PropertyField>().Where(p => p.bindingPath == path).First();
                if (pf == null) continue;
                pf.RegisterValueChangeCallback(_ =>
                {
                    RefreshValidation(detail, warnBox, item);
                    if (path == "attackAI") ApplyAttackFieldStates(detail, so, 0);
                });
            }
        }

        /// <summary>유효성 재판정 → 경고 박스 + 모순 필드 red 테두리 + 목록 행 ⚠ 갱신.</summary>
        void RefreshValidation(VisualElement detail, VisualElement warnBox, EnemyDefinition item)
        {
            var warns = ValidateAll(item);
            RenderWarningBox(warnBox, warns);
            ApplyFieldHighlights(detail, warns);
            RefreshRows();   // 목록 ⚠ 컬럼 갱신
        }

        /// <summary>R1·R2(<see cref="EnemyValidation"/>) + R3(§6 비주얼 Addressables 라벨 교차, 에디터 전용) 통합 판정.</summary>
        static List<EnemyWarning> ValidateAll(EnemyDefinition def)
        {
            var list = EnemyValidation.Validate(def);
            AppendLabelWarning(list, def);
            return list;
        }

        // Archetype → 비주얼 프리팹의 Addressables `archetype:` 라벨 문자열. (라벨층은 아직 한글 '탄막' 등 — code enum과 분리, backlog M2-2a 참조.)
        static readonly Dictionary<Archetype, string> ArchetypeLabel = new Dictionary<Archetype, string>
        {
            { Archetype.Shooter, "archetype:탄막" },
            { Archetype.Charger, "archetype:돌진" },
            { Archetype.Bomber,  "archetype:자폭" },
            { Archetype.Hybrid,  "archetype:복합" },
        };

        /// <summary>R3(§6) — 배정된 비주얼의 `archetype:` 라벨 집합에 SO.archetype이 없으면 "부자연스러운 조합" 경고. 미배정/비Addressable/라벨없음은 스킵.</summary>
        static void AppendLabelWarning(List<EnemyWarning> list, EnemyDefinition def)
        {
            if (def == null || def.visual == null) return;
            string guid = def.visual.AssetGUID;
            if (string.IsNullOrEmpty(guid)) return;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;
            var entry = settings.FindAssetEntry(guid);
            if (entry == null) return;

            string want = ArchetypeLabel.TryGetValue(def.archetype, out var w) ? w : null;
            bool hasAny = false, match = false;
            foreach (var lbl in entry.labels)
            {
                if (!lbl.StartsWith("archetype:")) continue;
                hasAny = true;
                if (lbl == want) match = true;
            }
            if (hasAny && !match)
                list.Add(new EnemyWarning(
                    $"비주얼의 적합 아키타입 집합에 없는 {def.archetype} — 부자연스러운 조합(비주얼 부조화).",
                    "visual", "archetype"));
        }

        /// <summary>경고 목록을 박스에 렌더. 없으면 박스 숨김.</summary>
        static void RenderWarningBox(VisualElement box, List<EnemyWarning> warns)
        {
            box.Clear();
            if (warns.Count == 0)
            {
                box.style.display = DisplayStyle.None;
                return;
            }
            box.style.display = DisplayStyle.Flex;

            var header = new Label($"⚠ 유효성 경고 {warns.Count}");
            header.AddToClassList("so-warning-header");
            box.Add(header);
            foreach (var w in warns)
            {
                var line = new Label("• " + w.Message);
                line.AddToClassList("so-warning-item");
                box.Add(line);
            }
        }

        /// <summary>경고의 관련 필드(<see cref="EnemyWarning.Fields"/>)에 red 테두리 클래스 토글.</summary>
        static void ApplyFieldHighlights(VisualElement detail, List<EnemyWarning> warns)
        {
            var offending = new HashSet<string>();
            foreach (var w in warns)
                foreach (var f in w.Fields) offending.Add(f);

            foreach (var pf in detail.Query<PropertyField>().ToList())
            {
                if (pf.bindingPath == null) continue;
                pf.EnableInClassList("so-field-error", offending.Contains(pf.bindingPath));
            }
        }

        void ApplyAttackFieldStates(VisualElement detail, SerializedObject so, int retries)
        {
            var pfs = detail.Query<PropertyField>().ToList();

            int found = 0;
            foreach (var p in pfs)
                if (p.bindingPath != null && Array.IndexOf(AttackStatPaths, p.bindingPath) >= 0) found++;
            if (found < AttackStatPaths.Length && retries > 0)   // 아직 안 빌드됨 → 다음 틱 재시도
            {
                detail.schedule.Execute(() => ApplyAttackFieldStates(detail, so, retries - 1)).ExecuteLater(16);
                return;
            }

            var relevant = RelevantAttackStats((AttackAIType)so.FindProperty("attackAI").enumValueIndex);
            foreach (var p in pfs)
                if (p.bindingPath != null && Array.IndexOf(AttackStatPaths, p.bindingPath) >= 0)
                    p.SetEnabled(relevant.Contains(p.bindingPath));
        }

        /// <summary>AttackAI가 실제로 쓰는 stats 필드 집합(enemy-design §3). 충돌=없음.</summary>
        static HashSet<string> RelevantAttackStats(AttackAIType ai)
        {
            switch (ai)
            {
                case AttackAIType.AimedShot: return new HashSet<string> { "stats.fireInterval", "stats.projectileSpeed" };
                case AttackAIType.Barrage:   return new HashSet<string> { "stats.fireInterval", "stats.projectileSpeed", "stats.barrageCount" };
                case AttackAIType.Suicide:   return new HashSet<string> { "stats.suicideRadius" };
                default:                     return new HashSet<string>();   // Contact(충돌): 없음
            }
        }
    }
}
