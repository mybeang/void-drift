using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 기관총(M4-1) — 조준 원뿔 안 최근접 적으로 스냅(없으면 축 직사)해 직진 단일 히트 투사체(<see cref="Projectile"/>)를 연사한다.
    /// 기존 <see cref="PlayerShooter"/> 하드코딩 발사를 전략 모듈로 이관(<b>동작 무변화</b>). 쿨다운=인스턴스 상태.
    /// <para><b>탄약(M4-2)</b> = <see cref="WeaponBase.Ammo"/>(=레벨) — 조준 축에 수직으로 나란히(평행 오프셋, 레일건식) 그 수만큼 동시 발사.
    /// Lv1=1발 … Lv4=4발. 줄기 간격=<see cref="_streamSpacing"/>.</para>
    /// 수치는 생성자 주입(코드 기본값 — 후일 SO화 가능).
    /// </summary>
    public sealed class StraightGun : WeaponBase
    {
        readonly float _fireInterval;
        readonly float _projectileSpeed;
        readonly float _projectileLifetime;
        readonly float _coneHalfAngle;
        readonly float _aimRange;
        readonly float _streamSpacing;

        float _cooldown;

        public StraightGun(float fireInterval, float projectileSpeed, float projectileLifetime, float coneHalfAngle, float aimRange, float streamSpacing, int startLevel)
            : base(startLevel)
        {
            _fireInterval = fireInterval;
            _projectileSpeed = projectileSpeed;
            _projectileLifetime = projectileLifetime;
            _coneHalfAngle = coneHalfAngle;
            _aimRange = aimRange;
            _streamSpacing = streamSpacing;
        }

        public override void Tick(float dt, WeaponContext ctx)
        {
            _cooldown -= dt;
            if (_cooldown > 0f) return;
            if (ctx.StraightPool == null || ctx.FirePoint == null) return;
            _cooldown = _fireInterval / FireRateMult;   // 연사 강화(M4-8)
            AudioManager.Instance?.PlaySfx(SfxId.BasicGun, ctx.FirePoint.position);   // 발사음(M5-5)

            // 조준 방향 = 조준 축(FirePoint.forward). 원뿔 안에 적 있으면 그 적으로 스냅.
            Vector3 dir = ctx.FirePoint.forward;
            if (ctx.TryConeTarget(_aimRange, _coneHalfAngle, out Vector3 aimPoint))
                dir = (aimPoint - ctx.FirePoint.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            // 탄약 수만큼 조준 축에 수직으로 나란히 배치(가운데 정렬, 레일건과 동일 규칙).
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            if (side.sqrMagnitude < 1e-4f) side = ctx.FirePoint.right;
            side.Normalize();

            int n = Ammo;
            float startOffset = -(n - 1) * 0.5f * _streamSpacing;
            for (int i = 0; i < n; i++)
            {
                Vector3 origin = ctx.FirePoint.position + side * (startOffset + i * _streamSpacing);
                Projectile p = ctx.StraightPool.Get();
                p.transform.SetPositionAndRotation(origin, rot);
                p.Launch(_projectileSpeed * ProjectileSpeedMult, _projectileLifetime, ctx.BaseDamage);   // 탄속 강화(M4-8)
            }
        }
    }
}
