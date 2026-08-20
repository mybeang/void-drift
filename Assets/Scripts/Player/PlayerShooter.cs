using System.Collections.Generic;
using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 오토 사격 오케스트레이터(M4-1) — 보유 <see cref="IWeapon"/> 리스트를 <see cref="GameState.Playing"/>에서 매 프레임 틱해
    /// 각 무기가 자기 연사속도로 <b>동시 오토발사</b>하게 한다(weapon-acquisition §1). 공용 조준·풀·base 데미지는 <see cref="WeaponContext"/>로 주입.
    /// <para>M4-1 스코프: 3무기 동시발사 골격. <b>Step1 = 기관총 전략 모듈 리팩터(동작 무변화)</b> — 유도/레일건=Step2/3, 획득/마일스톤 전환=Step5/M4-3.
    /// 인스펙터 수치(기관총 튜닝)는 그대로 유지, Awake에서 <see cref="StraightGun"/> 구성에 주입한다.</para>
    /// </summary>
    public sealed class PlayerShooter : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("발사 원점·방향(조준 축). 보통 Player/FirePoint")]
        [SerializeField] Transform firePoint;
        [Tooltip("직진 투사체 풀(기관총). 비우면 씬에서 자동 탐색")]
        [SerializeField] ProjectilePool pool;

        [Header("기관총 (탄속·발사속도 등 튜닝은 여기 한 곳에)")]
        [Tooltip("발사 간격(초). 작을수록 빠른 연사. 수치는 Day5 튜닝")]
        [SerializeField] float fireInterval = 0.15f;
        [Tooltip("탄속(월드 유닛/초). 발사 시 투사체에 주입. 수치는 Day5 튜닝")]
        [SerializeField] float projectileSpeed = 40f;
        [Tooltip("투사체 수명(초). 지나면 풀 반납")]
        [SerializeField] float projectileLifetime = 3f;
        [Tooltip("기초 공격력(무기 공통 base). M3-4 강화가 가산, M4-8 무기 배율이 곱함. 수치는 Day5 튜닝")]
        [SerializeField] float projectileDamage = 10f;

        [Header("조준 (원뿔 타겟 스냅 — 기관총)")]
        [Tooltip("조준 원뿔 반각(도). 이 안의 적에게 스냅해 발사. 밖이면 조준 축(FirePoint.forward) 직사")]
        [SerializeField] float aimConeHalfAngle = 25f;
        [Tooltip("타겟 탐색 사거리(월드)")]
        [SerializeField] float aimRange = 90f;
        [Tooltip("타겟 레이어(레이어 미설정 시 전체). 실제 적 판별은 IDamageable로 추가 필터")]
        [SerializeField] LayerMask targetMask = ~0;
        [Tooltip("[임시] 조준 원뿔 기즈모 표시(Scene/Game). 검증·튜닝 후 제거")]
        [SerializeField] bool drawAimGizmo = true;

        readonly List<IWeapon> _weapons = new();
        WeaponContext _ctx;

        void Awake()
        {
            if (firePoint == null) firePoint = transform;
            if (pool == null) pool = FindAnyObjectByType<ProjectilePool>();

            _ctx = new WeaponContext
            {
                FirePoint = firePoint,
                TargetMask = targetMask,
                StraightPool = pool,
                BaseDamage = projectileDamage,
            };

            // M4-1 Step1: 기관총만(리팩터·동작 무변화). 유도/레일건=Step2/3에서 _weapons에 추가.
            _weapons.Add(new StraightGun(fireInterval, projectileSpeed, projectileLifetime, aimConeHalfAngle, aimRange));
        }

        /// <summary>기초 공격력(무기 공통 base) 가산 강화 — M3-4(3choice). 무기별 배율은 M4-8에서 이 base에 곱함.</summary>
        public void AddAttackPower(float amount)
        {
            projectileDamage += amount;
        }

        void Update()
        {
            if (!IsPlaying() || firePoint == null) return;

            // 컨텍스트를 매 프레임 최신화(base 데미지 강화·풀 재탐색 반영) 후 보유 무기 전부 틱 → 동시 오토발사.
            _ctx.FirePoint = firePoint;
            _ctx.TargetMask = targetMask;
            _ctx.StraightPool = pool;
            _ctx.BaseDamage = projectileDamage;

            float dt = Time.deltaTime;
            for (int i = 0; i < _weapons.Count; i++)
                _weapons[i].Tick(dt, _ctx);
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            // GameManager 없이 단독 테스트 시엔 동작 허용(PlayerMovement와 동일 정책).
            return gm == null || gm.State == GameState.Playing;
        }

        // [임시] 조준 원뿔 시각화(기관총) — 축 + 사거리 끝 링 + 스포크. 검증·튜닝 후 제거.
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
