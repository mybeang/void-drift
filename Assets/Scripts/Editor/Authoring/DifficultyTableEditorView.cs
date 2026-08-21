using System;
using UnityEngine.UIElements;
using VD.Core;

namespace VD.Editor
{
    /// <summary>
    /// 난이도 페이즈 오서링 뷰 (M4-5) — 재사용 베이스 <see cref="SoTableEditorView{T}"/>에 <see cref="DifficultyPhaseDefinition"/> 바인딩.
    /// 세 번째 Table Tool(적·강화에 이어) — 같은 베이스에 도메인 컬럼만 얹어 저렴하게 확장(관심사 분리).
    /// 상세 편집(지속시간·시작/끝 배율·안내문구)은 베이스가 SerializedObject를 PropertyField로 자동 나열.
    /// 순서(시간축)는 <see cref="DifficultyProvider"/>의 phases 배열이 정의 — 이 툴은 각 페이즈 값만 편집.
    /// </summary>
    public sealed class DifficultyTableEditorView : SoTableEditorView<DifficultyPhaseDefinition>
    {
        protected override string NewAssetBaseName => "Phase_New";

        protected override void ConfigureColumns(MultiColumnListView table)
        {
            base.ConfigureColumns(table);   // Name 컬럼
            AddTextColumn(table, "dur", "Dur(s)", d => d.durationSeconds.ToString("0.#"));
            AddTextColumn(table, "start", "Start×", d => d.startMultiplier.ToString("0.##"));
            AddTextColumn(table, "end", "End×", d => d.endMultiplier.ToString("0.##"));
            AddTextColumn(table, "banner", "Banner", d => string.IsNullOrEmpty(d.bannerText) ? "—" : Shorten(d.bannerText));
        }

        static string Shorten(string s) => s.Length <= 14 ? s : s.Substring(0, 13) + "…";

        void AddTextColumn(MultiColumnListView table, string name, string title, Func<DifficultyPhaseDefinition, string> get)
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
