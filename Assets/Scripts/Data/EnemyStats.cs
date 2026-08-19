using System;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 적 능력치 (enemy-design §7, M2-2b). <see cref="MoveAIType"/>/<see cref="AttackAIType"/>와 함께
    /// EnemyDefinition SO(M2-2c)에 묶이는 <b>순수 데이터</b> — 실제 수치는 Day5 밸런싱, 지금은 필드 틀만.
    /// 에디터 툴/인스펙터에서 편집(결정 #3)하므로 <c>[Serializable]</c> + 가변 필드(참조는 여기 없음).
    /// M1 하드코딩(<c>Enemy.maxHp</c>/<c>contactDamage</c>/<c>killScore</c>·<c>EnemySpawner.enemySpeed</c>)을 흡수.
    /// </summary>
    [Serializable]
    public struct EnemyStats
    {
        [Header("공통 (모든 적)")]
        [Tooltip("최대 체력. 스폰 시 이 값으로 리셋. ← Enemy.maxHp")]
        public float maxHp;

        [Tooltip("이동 속도(월드 유닛/초). 적별로 다름 → SO에서 읽어 주입. ← EnemySpawner.enemySpeed")]
        public float moveSpeed;

        [Tooltip("공격력. 전달 방식은 공격AI가 결정(충돌/돌진→접촉, 조준단발/탄막→탄별, 자폭→범위, 실배선 M3-2). ← Enemy.contactDamage")]
        public float damage;

        [Tooltip("실사망(처치) 시 주는 점수. ← Enemy.killScore")]
        public int killScore;

        [Header("공격AI별 (해당 공격AI일 때만 의미)")]
        [Tooltip("발사 간격(초). 조준단발/탄막에서 사용")]
        public float fireInterval;

        [Tooltip("적 탄속(월드 유닛/초). 조준단발/탄막에서 사용")]
        public float projectileSpeed;

        [Tooltip("한 번에 뿌리는 탄 수. 탄막에서 사용")]
        public int barrageCount;

        [Tooltip("자폭 폭발 반경(월드). 자폭에서 사용")]
        public float suicideRadius;
    }
}
