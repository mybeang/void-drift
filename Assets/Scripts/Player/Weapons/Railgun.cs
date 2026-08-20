using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 레일건 무기(M4-1 Step3) — 자기 쿨다운(느림·고관통)마다 조준 방향으로 <b>초고속 관통 라인</b>(<see cref="RailProjectile"/>)을 발사.
    /// 조준 = 기관총과 동일 원뿔 스냅(최근접, 없으면 축 직사). 타겟 없어도 발사(직선 빔).
    /// <para><b>탄약(M4-2)</b> = <see cref="WeaponBase.Ammo"/>(=레벨) — 동시 발사 레일 줄기 수, 조준 축에 수직으로 나란히 오프셋.
    /// <b>최대 관통 수</b>는 레벨 무관 별도 스택(기본값 주입, 업그레이드가 올림). 궤적 VFX는 프리팹 TrailRenderer.</para>
    /// 수치는 생성자 주입(코드 기본값 — 후일 SO화 가능).
    /// </summary>
    public sealed class Railgun : WeaponBase
    {
        readonly float _fireInterval;
        readonly float _projectileSpeed;
        readonly float _projectileLifetime;
        readonly int _maxPierce;
        readonly float _damageDecay;
        readonly float _coneHalfAngle;
        readonly float _aimRange;
        readonly float _streamSpacing;

        float _cooldown;

        public Railgun(float fireInterval, float projectileSpeed, float projectileLifetime, int maxPierce, float damageDecay, float coneHalfAngle, float aimRange, float streamSpacing, int startLevel)
            : base(startLevel)
        {
            _fireInterval = fireInterval;
            _projectileSpeed = projectileSpeed;
            _projectileLifetime = projectileLifetime;
            _maxPierce = maxPierce;
            _damageDecay = damageDecay;
            _coneHalfAngle = coneHalfAngle;
            _aimRange = aimRange;
            _streamSpacing = streamSpacing;
        }

        public override void Tick(float dt, WeaponContext ctx)
        {
            _cooldown -= dt;
            if (_cooldown > 0f) return;
            if (ctx.RailPool == null || ctx.FirePoint == null) return;
            _cooldown = _fireInterval;

            // 조준 방향 = 축(FirePoint.forward). 원뿔 안에 적 있으면 스냅. 타겟 없어도 직선 발사.
            Vector3 dir = ctx.FirePoint.forward;
            if (ctx.TryConeTarget(_aimRange, _coneHalfAngle, out Vector3 aimPoint))
                dir = (aimPoint - ctx.FirePoint.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            // 탄약 수만큼 조준 축에 수직으로 나란히 배치(가운데 정렬).
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            if (side.sqrMagnitude < 1e-4f) side = ctx.FirePoint.right;
            side.Normalize();

            int n = Ammo;
            float startOffset = -(n - 1) * 0.5f * _streamSpacing;
            for (int i = 0; i < n; i++)
            {
                Vector3 origin = ctx.FirePoint.position + side * (startOffset + i * _streamSpacing);
                RailProjectile p = ctx.RailPool.Get();
                p.transform.SetPositionAndRotation(origin, rot);
                p.Launch(_projectileSpeed, _projectileLifetime, ctx.BaseDamage, _damageDecay, _maxPierce);
            }
        }
    }
}
