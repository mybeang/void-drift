using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 오토 사격 — <see cref="GameState.Playing"/> 상태에서 일정 간격으로 <see cref="firePoint"/> 방향으로 투사체를 발사한다(뱀서라이크).
    /// 발사 원점·방향 = FirePoint(= <see cref="PlayerAim"/>이 정렬한 조준 축). 무타겟이면 축 직사(원뿔 내 적 타겟 스냅은 M1-4).
    /// 데미지/충돌은 M1-4. (M1-3 3단계 신설)
    /// </summary>
    public sealed class PlayerShooter : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("발사 원점·방향(조준 축). 보통 Player/FirePoint")]
        [SerializeField] Transform firePoint;
        [Tooltip("투사체 풀. 비우면 씬에서 자동 탐색(프리팹은 씬 참조 불가라 런타임 탐색)")]
        [SerializeField] ProjectilePool pool;

        [Header("발사 (탄속·발사속도 등 튜닝은 여기 한 곳에)")]
        [Tooltip("발사 간격(초). 작을수록 빠른 연사. 수치는 Day5 튜닝")]
        [SerializeField] float fireInterval = 0.15f;
        [Tooltip("탄속(월드 유닛/초). 발사 시 투사체에 주입. 수치는 Day5 튜닝")]
        [SerializeField] float projectileSpeed = 40f;
        [Tooltip("투사체 수명(초). 지나면 풀 반납. 발사 시 투사체에 주입")]
        [SerializeField] float projectileLifetime = 3f;

        float _cooldown;

        void Awake()
        {
            if (firePoint == null) firePoint = transform;
            if (pool == null) pool = FindAnyObjectByType<ProjectilePool>();
        }

        void Update()
        {
            if (!IsPlaying() || pool == null || firePoint == null) return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;
            _cooldown = fireInterval;

            Projectile p = pool.Get();
            p.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
            p.Launch(projectileSpeed, projectileLifetime);
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            // GameManager 없이 단독 테스트 시엔 동작 허용(PlayerMovement와 동일 정책).
            return gm == null || gm.State == GameState.Playing;
        }
    }
}
