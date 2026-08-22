using System;
using UnityEngine.UIElements;
using VD.Core;

namespace VD.Editor
{
    /// <summary>
    /// 스폰 프로파일 오서링 뷰 (M4-6, enemy-design §5 3층) — 재사용 베이스 <see cref="SoTableEditorView{T}"/>에
    /// <see cref="SpawnProfileDefinition"/> 바인딩. 네 번째 Table Tool(적·강화·난이도에 이어).
    /// 상세(적 조합 테이블·밀도)는 베이스가 SerializedObject를 PropertyField로 자동 나열.
    /// 페이즈↔프로파일 연결은 Difficulty Authoring의 페이즈 `spawnProfile` 필드가 정의.
    /// </summary>
    public sealed class SpawnProfileTableEditorView : SoTableEditorView<SpawnProfileDefinition>
    {
        protected override string NewAssetBaseName => "Spawn_New";

        protected override void ConfigureColumns(MultiColumnListView table)
        {
            base.ConfigureColumns(table);   // Name 컬럼
            AddTextColumn(table, "maxWave", "~Wave", d => "~" + d.maxWave);
            AddTextColumn(table, "count", "적종수", d => ValidCount(d).ToString());
        }

        static int ValidCount(SpawnProfileDefinition d)
        {
            int n = 0;
            if (d.table != null)
                foreach (var e in d.table)
                    if (e.def != null && e.weight > 0f) n++;
            return n;
        }

        void AddTextColumn(MultiColumnListView table, string name, string title, Func<SpawnProfileDefinition, string> get)
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
