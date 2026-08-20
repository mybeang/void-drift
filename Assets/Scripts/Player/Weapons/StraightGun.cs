using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 기관총(M4-1) — 조준 원뿔 안 최근접 적으로 스냅(없으면 축 직사)해 직진 단일 히트 투사체(<see cref="Projectile"/>)를 연사한다.
    /// 기존 <see cref="PlayerShooter"/> 하드코딩 발사를 전략 모듈로 이관(<b>동작 무변화</b>). 쿨다운=인스턴스 상태.
    /// 수치는 생성자 주입(코드 기본값 — M4-2에서 SO화 가능).
    /// </summary>
    public sealed class StraightGun : IWeapon
    {
        readonly float _fireInterval;
        readonly float _projectileSpeed;
        readonly float _projectileLifetime;
        readonly float _coneHalfAngle;
        readonly float _aimRange;

        float _cooldown;

        public StraightGun(float fireInterval, float projectileSpeed, float projectileLifetime, float coneHalfAngle, float aimRange)
        {
            _fireInterval = fireInterval;
            _projectileSpeed = projectileSpeed;
            _projectileLifetime = projectileLifetime;
            _coneHalfAngle = coneHalfAngle;
            _aimRange = aimRange;
        }

        public void Tick(float dt, WeaponContext ctx)
        {
            _cooldown -= dt;
            if (_cooldown > 0f) return;
            if (ctx.StraightPool == null || ctx.FirePoint == null) return;
            _cooldown = _fireInterval;

            // 조준 방향 = 조준 축(FirePoint.forward). 원뿔 안에 적 있으면 그 적으로 스냅.
            Vector3 dir = ctx.FirePoint.forward;
            if (ctx.TryConeTarget(_aimRange, _coneHalfAngle, out Vector3 aimPoint))
                dir = (aimPoint - ctx.FirePoint.position).normalized;

            Projectile p = ctx.StraightPool.Get();
            p.transform.SetPositionAndRotation(ctx.FirePoint.position, Quaternion.LookRotation(dir, Vector3.up));
            p.Launch(_projectileSpeed, _projectileLifetime, ctx.BaseDamage);
        }
    }
}
