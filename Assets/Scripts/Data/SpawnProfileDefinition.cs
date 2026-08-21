using System;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 스폰 프로파일 SO (M4-6, enemy-design §5 3층) — 한 인스턴스 = 하나의 "적 조합 + 밀도" 큐레이션.
    /// <see cref="DifficultyPhaseDefinition.spawnProfile"/>로 페이즈에 연결되어, 그 페이즈 동안
    /// <see cref="EnemySpawner"/>가 <see cref="table"/>에서 가중 랜덤으로 적을 뽑고 <see cref="spawnInterval"/>로 밀도를 정한다.
    /// <para>페이즈 경계마다 프로파일이 바뀌면 "순한 조합 → 위협 조합" 전환이 됨(progression-design §2).
    /// 수치·조합은 Day5(오서링 툴 `Spawn Profile Authoring` 편집). 프로파일 미지정/빈 테이블이면 스포너가 기본 표로 폴백.</para>
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
