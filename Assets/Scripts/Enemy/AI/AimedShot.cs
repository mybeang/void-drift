using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 조준단발 공격(M3-3, 원래 M4-7 → 선반영). <see cref="Enemy.FireInterval"/>마다 플레이어 방향으로 <b>한 발</b>.
    /// 탄막(<see cref="BarrageAttack"/>)의 1발·부채꼴 없는 버전 — 공격 사다리의 "단순" 단계. `EnemyBullet` 재사용.
    /// 발사 쿨다운(per-instance 상태) → 빌더가 스폰마다 new. 탄속=<see cref="Enemy.ProjectileSpeed"/>, 데미지=<see cref="Enemy.Damage"/>.
    /// </summary>
    public sealed class AimedShot : IAttackBehaviour
    {
        readonly EnemyBulletPool _pool;
        readonly float _bulletLifetime;   // 적탄 수명(초). Day5 튜닝 / SO화 후보
        float _cooldown;

        public AimedShot(EnemyBulletPool pool, float bulletLifetime = 6f)
        {
            _pool = pool;
            _bulletLifetime = bulletLifetime;
        }

        public void OnSpawned()
        {
            _cooldown = 0f;   // 스폰 직후 한 번 쏘고 시작
        }

        public void Tick(Enemy self, float dt)
        {
            if (_pool == null) return;

            _cooldown -= dt;
            if (_cooldown > 0f) return;
            _cooldown = Mathf.Max(0.05f, self.FireInterval);

            Vector3 origin = self.transform.position;
            Transform target = PlayerLocator.Get();
            Vector3 dir = target != null ? (target.position - origin) : Vector3.back;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.back;

            EnemyBullet b = _pool.Get();
            b.transform.position = origin;
            b.Launch(dir, self.ProjectileSpeed, _bulletLifetime, self.Damage);
        }
    }
}
