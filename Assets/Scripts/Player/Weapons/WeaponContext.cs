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

        /// <summary>직진 투사체 풀(기관총). 무기별 전용 풀(유도/레일건)은 각 무기 단계에서 컨텍스트에 추가.</summary>
        public ProjectilePool StraightPool;

        static readonly Collider[] _overlap = new Collider[32];

        /// <summary>
        /// 조준 축(<see cref="FirePoint"/>.forward) 기준 반각 <paramref name="halfAngleDeg"/>·사거리 <paramref name="range"/>
        /// 원뿔 안에서 가장 가까운 <see cref="IDamageable"/>를 조준점으로 반환(락 아님, 매 발 재탐색). 없으면 false.
        /// (기존 <see cref="PlayerShooter"/>.TryAcquireTarget 로직 이관.)
        /// </summary>
        public bool TryConeTarget(float range, float halfAngleDeg, out Vector3 aimPoint)
        {
            aimPoint = default;
            if (FirePoint == null) return false;

            Vector3 origin = FirePoint.position;
            Vector3 axis = FirePoint.forward;
            int n = Physics.OverlapSphereNonAlloc(origin, range, _overlap, TargetMask, QueryTriggerInteraction.Collide);
            float bestDist = float.MaxValue;
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                var col = _overlap[i];
                if (col == null || col.GetComponentInParent<IDamageable>() == null) continue;
                Vector3 c = col.bounds.center;
                Vector3 to = c - origin;
                float dist = to.magnitude;
                if (dist < 0.01f || Vector3.Angle(axis, to) > halfAngleDeg) continue;
                if (dist < bestDist) { bestDist = dist; aimPoint = c; found = true; }
            }
            return found;
        }
    }
}
