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
        [Tooltip("투사체 데미지. 히트 시 IDamageable에 전달. 수치는 Day5 튜닝")]
        [SerializeField] float projectileDamage = 10f;

        [Header("조준 (원뿔 타겟 스냅)")]
        [Tooltip("조준 원뿔 반각(도). 이 안의 적에게 스냅해 발사. 밖이면 조준 축(FirePoint.forward) 직사")]
        [SerializeField] float aimConeHalfAngle = 25f;
        [Tooltip("타겟 탐색 사거리(월드)")]
        [SerializeField] float aimRange = 90f;
        [Tooltip("타겟 레이어(레이어 미설정 시 전체). 실제 적 판별은 IDamageable로 추가 필터")]
        [SerializeField] LayerMask targetMask = ~0;
        [Tooltip("[임시] 조준 원뿔 기즈모 표시(Scene/Game). 검증·튜닝 후 제거")]
        [SerializeField] bool drawAimGizmo = true;

        float _cooldown;
        static readonly Collider[] _overlap = new Collider[32];

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

            // 조준 방향 = 조준 축(FirePoint.forward). 원뿔 안에 적 있으면 그 적으로 스냅.
            Vector3 dir = firePoint.forward;
            if (TryAcquireTarget(out Vector3 aimPoint))
                dir = (aimPoint - firePoint.position).normalized;

            Projectile p = pool.Get();
            p.transform.SetPositionAndRotation(firePoint.position, Quaternion.LookRotation(dir, Vector3.up));
            p.Launch(projectileSpeed, projectileLifetime, projectileDamage);
        }

        /// <summary>원뿔(반각 aimConeHalfAngle·사거리 aimRange) 안에서 가장 가까운 IDamageable을 조준점으로.</summary>
        bool TryAcquireTarget(out Vector3 aimPoint)
        {
            aimPoint = default;
            int n = Physics.OverlapSphereNonAlloc(firePoint.position, aimRange, _overlap, targetMask, QueryTriggerInteraction.Collide);
            Vector3 axis = firePoint.forward;
            float bestDist = float.MaxValue;
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                var col = _overlap[i];
                if (col == null || col.GetComponentInParent<IDamageable>() == null) continue;
                Vector3 c = col.bounds.center;
                Vector3 to = c - firePoint.position;
                float dist = to.magnitude;
                if (dist < 0.01f || Vector3.Angle(axis, to) > aimConeHalfAngle) continue;
                if (dist < bestDist) { bestDist = dist; aimPoint = c; found = true; }
            }
            return found;
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            // GameManager 없이 단독 테스트 시엔 동작 허용(PlayerMovement와 동일 정책).
            return gm == null || gm.State == GameState.Playing;
        }

        // [임시] 조준 원뿔 시각화 — 축 + 사거리 끝 링 + 스포크. 검증·튜닝 후 제거.
        void OnDrawGizmos()
        {
            if (!drawAimGizmo || firePoint == null) return;

            Vector3 o = firePoint.position;
            Vector3 axis = firePoint.forward;
            float a = aimConeHalfAngle * Mathf.Deg2Rad;
            float ca = Mathf.Cos(a), sa = Mathf.Sin(a);

            Vector3 up = Vector3.Cross(axis, Vector3.up);
            if (up.sqrMagnitude < 1e-4f) up = Vector3.Cross(axis, Vector3.right);
            up.Normalize();
            Vector3 right = Vector3.Cross(axis, up).normalized;

            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            Gizmos.DrawLine(o, o + axis * aimRange);   // 조준 축

            const int seg = 32;
            Vector3 prev = Vector3.zero;
            for (int i = 0; i <= seg; i++)
            {
                float t = (i / (float)seg) * Mathf.PI * 2f;
                Vector3 radial = up * Mathf.Cos(t) + right * Mathf.Sin(t);
                Vector3 dir = axis * ca + radial * sa;   // 원뿔 표면 방향
                Vector3 p = o + dir * aimRange;          // 사거리(구 반지름) 위 점
                if (i > 0) Gizmos.DrawLine(prev, p);     // 사거리 끝 링
                if (i % 8 == 0) Gizmos.DrawLine(o, p);   // 스포크 4개
                prev = p;
            }
        }
    }
}
