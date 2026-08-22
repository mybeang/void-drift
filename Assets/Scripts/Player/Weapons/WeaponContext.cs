using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 무기 모듈 공용 컨텍스트(M4-1). <see cref="PlayerShooter"/>가 매 프레임 갱신해 각 <see cref="IWeapon"/>에 주입한다 —
    /// 무기가 개별로 재탐색하지 않도록 발사 원점·base 데미지·타겟 질의·풀을 한곳에 모은다.
    /// 조준은 두 갈래로 제공: 원뿔 스냅(기관총 등)과 최근접(유도 미사일, 원뿔 무관 — Step2에서 추가).
    /// </summary>
    public sealed class WeaponContext
    {
        /// <summary>발사 원점·조준 축(FirePoint). <see cref="PlayerShooter"/>가 배선.</summary>
        public Transform FirePoint;

        /// <summary>기초 공격력(무기 공통 base). M3-4 <see cref="PlayerShooter.AddAttackPower"/>가 키우고, M4-8 무기 배율이 여기에 곱함.</summary>
        public float BaseDamage;

        /// <summary>타겟 판별 레이어(적). 실제 적 여부는 <see cref="IDamageable"/>로 추가 필터.</summary>
        public LayerMask TargetMask;

        /// <summary>직진 투사체 풀(기관총). 무기별 전용 풀(레일건 등)은 각 무기 단계에서 컨텍스트에 추가.</summary>
        public ProjectilePool StraightPool;

        /// <summary>유도 미사일 전용 풀(Step2).</summary>
        public HomingProjectilePool HomingPool;

        /// <summary>유도 미사일 발사대(날개 하드포인트). 배열 순서 = 발사 순서(1→2→3→4 순환). 비면 <see cref="FirePoint"/> 폴백.</summary>
        public Transform[] HomingHardpoints;

        /// <summary>레일건 투사체 전용 풀(Step3).</summary>
        public RailProjectilePool RailPool;

        static readonly Collider[] _overlap = new Collider[32];

        /// <summary>
        /// 조준 축(<see cref="FirePoint"/>.forward) 기준 <b>프러스텀</b>(시작 반경 <paramref name="startRadius"/> + 반각 <paramref name="halfAngleDeg"/>)·
        /// 사거리 <paramref name="range"/> 안에서 가장 가까운 <see cref="IDamageable"/>를 조준점으로 반환(락 아님, 매 발 재탐색). 없으면 false.
        /// <para>축에서의 허용 수직거리 = <c>startRadius + tan(halfAngle)×축방향거리</c> — apex를 뾰족하게 두지 않고
        /// 시작 직경을 넓혀 근접 오조준을 줄인다(<paramref name="startRadius"/>=0이면 순수 원뿔=기존 동작).</para>
        /// </summary>
        public bool TryConeTarget(float range, float halfAngleDeg, float startRadius, out Vector3 aimPoint)
        {
            aimPoint = default;
            if (FirePoint == null) return false;

            Vector3 origin = FirePoint.position;
            Vector3 axis = FirePoint.forward;
            float tan = Mathf.Tan(halfAngleDeg * Mathf.Deg2Rad);
            int n = Physics.OverlapSphereNonAlloc(origin, range, _overlap, TargetMask, QueryTriggerInteraction.Collide);
            float bestDist = float.MaxValue;
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                var col = _overlap[i];
                if (col == null || col.GetComponentInParent<IDamageable>() == null) continue;
                Vector3 c = col.bounds.center;
                Vector3 to = c - origin;
                float axial = Vector3.Dot(to, axis);              // 축방향 성분
                if (axial < 0f || axial > range) continue;         // 뒤/사거리 밖 제외
                float perp = (to - axis * axial).magnitude;        // 축에서 수직 거리
                if (perp > startRadius + tan * axial) continue;    // 프러스텀 밖
                float dist = to.magnitude;
                if (dist < bestDist) { bestDist = dist; aimPoint = c; found = true; }
            }
            return found;
        }

        /// <summary>
        /// 유도 미사일 조준 타겟(원뿔 무관, M4-1) — 사거리 <paramref name="range"/> 안 적 중
        /// <b>타입 1순위(원거리 우선) → 동급이면 가장 먼 것</b>으로 선택(사용자 결정 2026-08-20). 없으면 false.
        /// 원거리/근거리 = <see cref="VD.Core.Archetype"/> 파생(Shooter=원거리 최우선 · Hybrid=복합 · Charger/Bomber=근거리).
        /// </summary>
        public bool TryAcquireHomingTarget(float range, out Transform target)
        {
            target = null;
            if (FirePoint == null) return false;

            Vector3 origin = FirePoint.position;
            int n = Physics.OverlapSphereNonAlloc(origin, range, _overlap, TargetMask, QueryTriggerInteraction.Collide);
            int bestRank = int.MaxValue;
            float bestSqrDist = -1f;
            for (int i = 0; i < n; i++)
            {
                var col = _overlap[i];
                if (col == null) continue;
                var enemy = col.GetComponentInParent<VD.Enemy.Enemy>();
                if (enemy == null) continue;

                int rank = RangeRank(enemy.Archetype);
                float sqrDist = (enemy.transform.position - origin).sqrMagnitude;
                // 타입 1순위(rank 낮을수록 원거리 우선) → 동급이면 가장 먼 것(sqrDist 큰 것).
                if (rank < bestRank || (rank == bestRank && sqrDist > bestSqrDist))
                {
                    bestRank = rank;
                    bestSqrDist = sqrDist;
                    target = enemy.transform;
                }
            }
            return target != null;
        }

        /// <summary>아키타입 → 조준 우선순위 랭크(낮을수록 우선). 원거리(Shooter)=0 · 복합=1 · 근거리=2.</summary>
        static int RangeRank(VD.Core.Archetype a) => a switch
        {
            VD.Core.Archetype.Shooter => 0,
            VD.Core.Archetype.Hybrid => 1,
            _ => 2,
        };
    }
}
