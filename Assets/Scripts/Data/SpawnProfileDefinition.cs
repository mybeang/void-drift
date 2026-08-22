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

        [Header("적 조합 (가중 랜덤)")]
        [Tooltip("이 프로파일에서 출현할 적 + 가중치. 비우면 스포너 기본 표로 폴백")]
        public Entry[] table = Array.Empty<Entry>();

        [Header("밀도")]
        [Tooltip("스폰 간격(초). 작을수록 조밀. 0 이하면 스포너 기본 간격 사용")]
        public float spawnInterval = 1f;
    }
}
