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
        readonly float _engageNearZ;      // 교전 라인(플레이어 앞 Z갭) 근접 경계 — 통과 시 발사 중지. Day5
        readonly float _engageFarZ;       // 교전 라인 원거리 경계 — 이보다 멀면 발사 안 함(I-4). Day5
        float _cooldown;

        public AimedShot(EnemyBulletPool pool, float bulletLifetime = 6f,
            float engageNearZ = 10f, float engageFarZ = 60f)
        {
            _pool = pool;
            _bulletLifetime = bulletLifetime;
            _engageNearZ = engageNearZ;
            _engageFarZ = engageFarZ;
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

            // 교전 라인 게이팅(I-4): 플레이어 앞 [nearZ, farZ] 구간에서만 발사.
            // 밖이면 쿨다운 유지 → 윈도우 진입 즉시 발사.
            if (!InEngageWindow(self)) return;

            _cooldown = Mathf.Max(0.05f, self.FireInterval);

            Vector3 origin = self.transform.position;
            Transform target = PlayerLocator.Get();
            Vector3 dir = target != null ? (target.position - origin) : Vector3.back;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.back;

            EnemyBullet b = _pool.Get();
            b.transform.position = origin;
            b.Launch(dir, self.ProjectileSpeed, _bulletLifetime, self.Damage);
        }

        // 플레이어 앞 Z갭이 교전 윈도우 안일 때만 발사(I-4).
        bool InEngageWindow(Enemy self)
        {
            Transform p = PlayerLocator.Get();
            if (p == null) return true;
            float dz = self.transform.position.z - p.position.z;
            return dz >= _engageNearZ && dz <= _engageFarZ;
        }
    }
}
