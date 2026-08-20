using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 유도 미사일 무기(M4-1 Step2) — 자기 쿨다운마다 사거리 안 적을 <b>타입 1순위(원거리 우선)→가장 먼 것</b>으로 조준(원뿔 무관)해
    /// 유도 투사체(<see cref="HomingProjectile"/>)를 발사한다. 타겟이 없으면 쏘지 않고 대기(쿨다운 유지 → 타겟 등장 즉시 발사).
    /// <para><b>탄약(M4-2)</b> = <see cref="WeaponBase.Ammo"/>(=레벨). 발사대(날개 하드포인트) 앞에서부터
    /// 탄약 수만큼 <b>동시</b> 발사 — Lv1=1번만, 레벨업하면 2·3·4번 순으로 추가. 하드포인트 없으면 FirePoint서 1발.</para>
    /// 탄속(가속도)·수명·선회율·Aim 사거리는 생성자 주입(코드 기본값 — Aim 사거리는 인스펙터 노출, 후일 SO화 가능).
    /// </summary>
    public sealed class HomingMissile : WeaponBase
    {
        readonly float _fireInterval;
        readonly float _initialSpeed;
        readonly float _acceleration;
        readonly float _maxSpeed;
        readonly float _projectileLifetime;
        readonly float _turnRate;
        readonly float _aimRange;

        float _cooldown;

        public HomingMissile(float fireInterval, float initialSpeed, float acceleration, float maxSpeed, float projectileLifetime, float turnRate, float aimRange, int startLevel)
            : base(startLevel)
        {
            _fireInterval = fireInterval;
            _initialSpeed = initialSpeed;
            _acceleration = acceleration;
            _maxSpeed = maxSpeed;
            _projectileLifetime = projectileLifetime;
            _turnRate = turnRate;
            _aimRange = aimRange;
        }

        public override void Tick(float dt, WeaponContext ctx)
        {
            _cooldown -= dt;
            if (_cooldown > 0f) return;
            if (ctx.HomingPool == null || ctx.FirePoint == null) return;
            if (!ctx.TryAcquireHomingTarget(_aimRange, out Transform target)) return;   // 타겟 없으면 대기(쿨다운 유지)
            _cooldown = _fireInterval;

            // 탄약 수만큼 발사대 앞에서부터 동시 발사. 하드포인트 없으면 FirePoint서 1발.
            var hps = ctx.HomingHardpoints;
            int available = hps != null ? hps.Length : 0;
            int count = available > 0 ? Mathf.Min(Ammo, available) : 1;

            for (int i = 0; i < count; i++)
            {
                Transform muzzle = (available > 0 && hps[i] != null) ? hps[i] : ctx.FirePoint;
                FireOne(ctx, muzzle, target);
            }
        }

        void FireOne(WeaponContext ctx, Transform muzzle, Transform target)
        {
            Vector3 origin = muzzle.position;
            Vector3 toTarget = target.position - origin;
            Quaternion rot = toTarget.sqrMagnitude > 1e-6f
                ? Quaternion.LookRotation(toTarget.normalized, Vector3.up)
                : ctx.FirePoint.rotation;

            HomingProjectile m = ctx.HomingPool.Get();
            m.transform.SetPositionAndRotation(origin, rot);
            m.Launch(_initialSpeed, _acceleration, _maxSpeed, _projectileLifetime, ctx.BaseDamage, _turnRate, target);
        }
    }
}
