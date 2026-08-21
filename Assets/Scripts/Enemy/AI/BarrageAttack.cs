using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 탄막 공격(M3-2). <see cref="Enemy.FireInterval"/>마다 플레이어 방향을 중심으로 <b>부채꼴</b> 스프레드로
    /// <see cref="Enemy.BarrageCount"/>발을 발사(사용자 결정: 부채꼴/조준). 탄속=<see cref="Enemy.ProjectileSpeed"/>,
    /// 데미지=<see cref="Enemy.Damage"/>. 부채꼴은 월드 Y축(화면 좌우) 기준 회전으로 펼침.
    /// <para>발사 쿨다운(per-instance 상태) 때문에 <b>인스턴스별</b>로 생성(빌더가 스폰마다 new). 총 스프레드 각/탄 수명은
    /// Day5 튜닝 값(SO화 후보). 적탄 풀은 생성 시 주입.</para>
    /// </summary>
    public sealed class BarrageAttack : IAttackBehaviour
    {
        readonly EnemyBulletPool _pool;
        readonly float _spreadAngle;    // 부채꼴 전체 각(도). Day5 튜닝 / SO화 후보
        readonly float _bulletLifetime; // 적탄 수명(초). Day5 튜닝 / SO화 후보
        readonly float _engageNearZ;    // 교전 라인(플레이어 앞 Z갭) 근접 경계 — 이보다 가까우면(통과) 발사 중지. Day5
        readonly float _engageFarZ;     // 교전 라인 원거리 경계 — 이보다 멀면 발사 안 함(먼 거리 난사 방지, I-4). Day5
        float _cooldown;

        public BarrageAttack(EnemyBulletPool pool, float spreadAngle = 50f, float bulletLifetime = 6f,
            float engageNearZ = 10f, float engageFarZ = 60f)
        {
            _pool = pool;
            _spreadAngle = spreadAngle;
            _bulletLifetime = bulletLifetime;
            _engageNearZ = engageNearZ;
            _engageFarZ = engageFarZ;
        }

        public void OnSpawned()
        {
            _cooldown = 0f;   // 스폰 직후 한 번 쏘고 시작(첫 발 지연을 원하면 FireInterval로)
        }

        public void Tick(Enemy self, float dt)
        {
            if (_pool == null) return;

            _cooldown -= dt;
            if (_cooldown > 0f) return;

            // 교전 라인 게이팅(I-4): 플레이어 앞 [nearZ, farZ] 구간에서만 발사.
            // 밖이면 쿨다운을 리셋하지 않아 윈도우 진입 즉시 발사된다.
            if (!InEngageWindow(self)) return;

            _cooldown = Mathf.Max(0.05f, self.FireInterval);   // 간격 0 방지

            Fire(self);
        }

        // 플레이어 앞 Z갭(enemy.z - player.z)이 교전 윈도우 안일 때만 발사 허용(I-4).
        // 너무 멀면(> farZ) 침묵, 라인 통과해 너무 가까우면(< nearZ) 발사 중지.
        bool InEngageWindow(Enemy self)
        {
            Transform p = PlayerLocator.Get();
            if (p == null) return true;   // 플레이어 없으면 제한 없음(단독 테스트)
            float dz = self.transform.position.z - p.position.z;
            return dz >= _engageNearZ && dz <= _engageFarZ;
        }

        void Fire(Enemy self)
        {
            Vector3 origin = self.transform.position;
            Transform target = PlayerLocator.Get();

            Vector3 baseDir = target != null ? (target.position - origin) : Vector3.back;
            if (baseDir.sqrMagnitude < 1e-6f) baseDir = Vector3.back;
            baseDir.Normalize();

            int n = Mathf.Max(1, self.BarrageCount);
            float half = _spreadAngle * 0.5f;
            float stepA = n > 1 ? _spreadAngle / (n - 1) : 0f;   // n발을 균등 분포

            for (int i = 0; i < n; i++)
            {
                float a = (n > 1) ? (-half + stepA * i) : 0f;
                Vector3 dir = Quaternion.AngleAxis(a, Vector3.up) * baseDir;   // 화면 좌우로 부채꼴

                EnemyBullet b = _pool.Get();
                b.transform.position = origin;
                b.Launch(dir, self.ProjectileSpeed, _bulletLifetime, self.Damage);
            }
        }
    }
}
