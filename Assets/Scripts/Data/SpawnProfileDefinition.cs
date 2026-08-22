using System;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 스폰 프로파일 SO (M4-6 → 밸런싱 패스 3-4에서 웨이브 밴드에 연결 예정) — 한 인스턴스 = 하나의 "적 조합 + 밀도" 큐레이션.
    /// <see cref="table"/>에서 가중 랜덤으로 적을 뽑고 <see cref="spawnInterval"/>로 밀도를 정한다.
    /// <para>구 페이즈 연결(<c>DifficultyPhaseDefinition</c>)은 폐기 — 3-4에서 <b>웨이브 밴드</b>별 프로파일로 재배선한다.
    /// 수치·조합은 오서링 툴 `Spawn Profile Authoring` 편집.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Spawn_", menuName = "Void Drift/Spawn Profile")]
    public sealed class SpawnProfileDefinition : ScriptableObject
    {
        /// <summary>스폰 후보 1행 — 적 정의 + 가중치(등장 확률). weight 0 이하는 제외.</summary>
        [Serializable]
        public struct Entry
        {
            public EnemyDefinition def;
            [Min(0f)] public float weight;
        }

        [Header("표시")]
        [Tooltip("프로파일 이름(툴/디버그 표시용). 로직 무관")]
        public string profileName;

        [Header("웨이브 밴드 (3-4)")]
        [Tooltip("이 밴드의 상한 웨이브(포함). 스포너는 현재 wave ≤ maxWave인 첫 밴드를 사용. 마지막 밴드(W60+)는 큰 값(예: 9999)")]
        [Min(1)] public int maxWave = 60;

        [Header("적 조합 (가중 랜덤 — 밴드 내에서 재정규화)")]
        [Tooltip("이 밴드에서 출현할 적 + 가중치. 비우면 스포너 기본 표로 폴백")]
        public Entry[] table = Array.Empty<Entry>();

        [Header("밀도 (미사용 — 주기는 WaveDifficultyConfig)")]
        [Tooltip("[deprecated] 3-1에서 소환 주기는 WaveDifficultyConfig로 이동. 이 값은 사용하지 않음")]
        public float spawnInterval = 1f;
    }
}
