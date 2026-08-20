using System;
using UnityEngine.UIElements;
using VD.Core;

namespace VD.Editor
{
    /// <summary>
    /// 3choice 강화 오서링 뷰 (M3-4) — 재사용 베이스 <see cref="SoTableEditorView{T}"/>에 <see cref="UpgradeDefinition"/> 바인딩.
    /// 두 번째 Table Tool(적 오서링에 이어) — 같은 베이스에 도메인 컬럼만 얹어 저렴하게 확장(관심사 분리).
    /// 상세 편집은 베이스가 SerializedObject를 PropertyField로 자동 나열(type/수치/가중치/스택 등 그대로 편집).
    /// </summary>
    public sealed class UpgradeTableEditorView : SoTableEditorView<UpgradeDefinition>
    {
        protected override string NewAssetBaseName => "Upgrade_New";

        protected override void ConfigureColumns(MultiColumnListView table)
        {
            base.ConfigureColumns(table);   // Name 컬럼
            AddTextColumn(table, "type", "Type", d => d.type.ToString());
            AddTextColumn(table, "effect", "Effect", d => d.EffectText);
            AddTextColumn(table, "weight", "Weight", d => d.weight.ToString("0.##"));
            AddTextColumn(table, "max", "MaxStk", d => d.maxStacks == 0 ? "무제한" : d.maxStacks.ToString());
        }

        void AddTextColumn(MultiColumnListView table, string name, string title, Func<UpgradeDefinition, string> get)
        {
            table.columns.Add(new Column
            {
                name = name,
                title = title,
                width = 92,
                makeCell = () => new Label(),
                bindCell = (e, i) => ((Label)e).text = Items[i] != null ? get(Items[i]) : string.Empty,
            });
        }
    }
}
