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
        [Tooltip("기관총 시작 레벨(1~4). 시작 로드아웃=기관총만(M4-3). 레벨=탄약(동시 발사 줄기 수). 보통 1")]
        [Range(1, 4)]
        [SerializeField] int straightStartLevel = 1;
        [Tooltip("기관총 줄기 간 간격(월드). 레벨>1일 때 나란한 탄 사이 폭")]
        [SerializeField] float straightStreamSpacing = 0.8f;

        [Header("조준 (원뿔 타겟 스냅 — 기관총)")]
        [Tooltip("조준 원뿔 반각(도). 이 안의 적에게 스냅해 발사. 밖이면 조준 축(FirePoint.forward) 직사")]
        [SerializeField] float aimConeHalfAngle = 25f;
        [Tooltip("타겟 탐색 사거리(월드)")]
        [SerializeField] float aimRange = 90f;
        [Tooltip("타겟 레이어(레이어 미설정 시 전체). 실제 적 판별은 IDamageable로 추가 필터")]
        [SerializeField] LayerMask targetMask = ~0;
        [Tooltip("[임시] 조준 원뿔 기즈모 표시(Scene/Game). 검증·튜닝 후 제거")]
        [SerializeField] bool drawAimGizmo = true;

        [Header("유도 미사일 (Step2)")]
        [Tooltip("유도 미사일 전용 풀. 비우면 씬에서 자동 탐색")]
        [SerializeField] HomingProjectilePool homingPool;
        [Tooltip("발사대(날개 하드포인트) 목록. 배열 앞에서부터 탄약 수만큼 동시 발사(1번→2번→…). 비면 FirePoint(중앙) 폴백")]
        [SerializeField] Transform[] homingHardpoints;
        [Tooltip("유도 발사 간격(초). 기관총보다 느리게. 수치 Day5")]
        [SerializeField] float homingFireInterval = 0.8f;
        [Tooltip("유도 초기 탄속(월드/초). 여기서 가속. 수치 Day5")]
        [SerializeField] float homingInitialSpeed = 12f;
        [Tooltip("유도 가속도(월드/초^2). 발사 후 점점 빨라짐. 수치 Day5")]
        [SerializeField] float homingAcceleration = 45f;
        [Tooltip("유도 최대 탄속(월드/초). 0이하=무제한. 수치 Day5")]
        [SerializeField] float homingMaxSpeed = 48f;
        [Tooltip("유도 수명(초)")]
        [SerializeField] float homingLifetime = 4f;
        [Tooltip("유도 선회율(도/초). 클수록 급선회. 수치 Day5")]
        [SerializeField] float homingTurnRate = 160f;
        [Tooltip("유도 Aim 사거리(월드). 이 안에서 타입 1순위(원거리 우선)→가장 먼 적을 조준. 나중에 플레이어 셋팅 툴로 이관")]
        [SerializeField] float homingAimRange = 90f;

        [Header("레일건 (Step3)")]
        [Tooltip("레일건 투사체 전용 풀. 비우면 씬에서 자동 탐색")]
        [SerializeField] RailProjectilePool railPool;
        [Tooltip("레일 발사 간격(초). 느리게. 수치 Day5")]
        [SerializeField] float railFireInterval = 1.0f;
        [Tooltip("레일 탄속(월드/초). 초고속. 수치 Day5")]
        [SerializeField] float railSpeed = 90f;
        [Tooltip("레일 수명(초)")]
        [SerializeField] float railLifetime = 2f;
        [Tooltip("최대 관통 수(한 줄기가 뚫는 몹 수). 레벨 무관 별도 스택, 최대 5 지향. 기본값")]
        [Range(1, 5)]
        [SerializeField] int railMaxPierce = 2;
        [Tooltip("관통당 데미지 곱(0.7 = -30%). Lv5 관통효율(M5-4)이 완화")]
        [Range(0.1f, 1f)]
        [SerializeField] float railDamageDecay = 0.7f;
        [Tooltip("레일 조준 원뿔 반각(도). 이 안 최근접에 스냅, 밖이면 축 직사")]
        [SerializeField] float railConeHalfAngle = 20f;
        [Tooltip("레일 조준 사거리(월드)")]
        [SerializeField] float railAimRange = 90f;
        [Tooltip("레일 줄기 간 간격(월드). 탄약>1일 때 나란한 레일 사이 폭")]
        [SerializeField] float railStreamSpacing = 1.5f;

        readonly List<IWeapon> _weapons = new();                       // 틱 순서(동시 오토발사)
        readonly Dictionary<WeaponId, IWeapon> _owned = new();          // 슬롯 조회(획득/레벨업/최대치 판정)
        WeaponContext _ctx;

        void Awake()
        {
            if (firePoint == null) firePoint = transform;
            if (pool == null) pool = FindAnyObjectByType<ProjectilePool>();
            if (homingPool == null) homingPool = FindAnyObjectByType<HomingProjectilePool>();
            if (railPool == null) railPool = FindAnyObjectByType<RailProjectilePool>();

            _ctx = new WeaponContext
            {
                FirePoint = firePoint,
                TargetMask = targetMask,
                StraightPool = pool,
                HomingPool = homingPool,
                HomingHardpoints = homingHardpoints,
                RailPool = railPool,
                BaseDamage = projectileDamage,
            };

            // M4-3: 시작 로드아웃 = 기관총만(weapon-acquisition §2). 유도/레일건은 5레벨 마일스톤 무기 카드로 획득.
            Acquire(WeaponId.Straight, straightStartLevel);
        }

        /// <summary>보유 무기를 <see cref="WeaponId"/>별 튜닝값으로 시작 레벨 <paramref name="startLevel"/>에 생성해 등록. 이미 있으면 무시.</summary>
        void Acquire(WeaponId id, int startLevel)
        {
            if (_owned.ContainsKey(id)) return;
            IWeapon w = BuildWeapon(id, startLevel);
            if (w == null) return;
            _owned[id] = w;
            _weapons.Add(w);
        }

        /// <summary>무기 인스턴스 팩토리 — 인스펙터 튜닝값 주입. 획득 시 startLevel=1(=탄약 1), 마일스톤 재픽업은 <see cref="IWeapon.LevelUp"/>.</summary>
        IWeapon BuildWeapon(WeaponId id, int startLevel) => id switch
        {
            WeaponId.Straight => new StraightGun(fireInterval, projectileSpeed, projectileLifetime, aimConeHalfAngle, aimRange, straightStreamSpacing, startLevel),
            WeaponId.Homing   => new HomingMissile(homingFireInterval, homingInitialSpeed, homingAcceleration, homingMaxSpeed, homingLifetime, homingTurnRate, homingAimRange, startLevel),
            WeaponId.Railgun  => new Railgun(railFireInterval, railSpeed, railLifetime, railMaxPierce, railDamageDecay, railConeHalfAngle, railAimRange, railStreamSpacing, startLevel),
            _ => null,
        };

        // ── 무기 슬롯 API (M4-3, 3choice 무기 카드가 사용) ────────────────────────────
        /// <summary>해당 무기 보유 여부.</summary>
        public bool HasWeapon(WeaponId id) => _owned.ContainsKey(id);

        /// <summary>현재 무기 레벨(미보유=0). 카드 표시(획득 vs Lv업)에 사용.</summary>
        public int WeaponLevel(WeaponId id) => _owned.TryGetValue(id, out IWeapon w) ? w.Level : 0;

        /// <summary>보유+최대레벨(Lv4) 도달 — 무기 카드 풀에서 제외 판정.</summary>
        public bool IsWeaponMaxed(WeaponId id) => _owned.TryGetValue(id, out IWeapon w) && w.IsMaxLevel;

        /// <summary>미보유면 획득(Lv1), 보유면 한 단계 레벨업. 마일스톤 무기 카드 선택 시 호출.</summary>
        public void AcquireOrLevelUp(WeaponId id)
        {
            if (_owned.TryGetValue(id, out IWeapon w)) w.LevelUp();
            else Acquire(id, 1);
        }

        // ── 무기별 파워 강화 (M4-8, 3choice 무기 파워 카드) — 보유 무기에만 적용 ──
        /// <summary>지정 무기 연사속도 강화(보유 시).</summary>
        public void AddWeaponFireRate(WeaponId id, float pct) { if (_owned.TryGetValue(id, out IWeapon w)) w.AddFireRate(pct); }
        /// <summary>지정 무기 탄속 강화(보유 시).</summary>
        public void AddWeaponProjectileSpeed(WeaponId id, float pct) { if (_owned.TryGetValue(id, out IWeapon w)) w.AddProjectileSpeed(pct); }
        /// <summary>레일건 최대 관통 수 강화(보유 시).</summary>
        public void AddRailgunPierce(int amount) { if (_owned.TryGetValue(WeaponId.Railgun, out IWeapon w) && w is Railgun r) r.AddPierce(amount); }

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
            _ctx.HomingPool = homingPool;
            _ctx.HomingHardpoints = homingHardpoints;
            _ctx.RailPool = railPool;
            _ctx.BaseDamage = projectileDamage;

            float dt = Time.deltaTime;
            for (int i = 0; i < _weapons.Count; i++)
                _weapons[i].Tick(dt, _ctx);
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            // GameManager 없이 단독 테스트 시엔 동작 허용(PlayerMovement와 동일 정책).
            return gm == null || (gm.State == GameState.Playing && !gm.CombatFrozen);
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
