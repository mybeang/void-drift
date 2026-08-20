using System;
using UnityEngine;
using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 엔티티(로직 셸, M2-5). 직진 접근 이동(-Z) + <see cref="IDamageable"/>(투사체 피격→HP 감소→사망).
    /// 사망·despawn 모두 풀에 반납한다(오브 드랍 M1-5·파괴 VFX M4-9는 이후). 플레이어 충돌 데미지는 M1-9.
    /// <para>능력치는 <see cref="EnemyStats"/> 하나로 데이터화(M2-5b) — 스폰 시 빌더가 <see cref="ApplyStats"/>로
    /// <b>effective(base×전역배율)</b>를 주입(M2-5d/e). 인스펙터 <see cref="stats"/>는 미주입 시 폴백 기본값.</para>
    /// 비주얼은 프리팹에 없고 빌더가 자식으로 주입(M2-5c). despawn 기준은 스포너가 <see cref="Launch"/>로 주입.
    /// 이동은 AI 모듈(<see cref="IMoveBehaviour"/>, M3-1)에 위임 — 빌더가 def.moveAI로 주입, Update가 매 프레임 Tick 창구.
    /// </summary>
    public sealed class Enemy : MonoBehaviour, IDamageable
    {
        [Tooltip("능력치(체력/이동속도/데미지/처치점수 + 공격AI별). 스폰 시 빌더가 ApplyStats로 effective 주입 — 이 인스펙터 값은 미주입 시 폴백 기본값. 수치 Day5")]
        [SerializeField] EnemyStats stats;

        /// <summary>플레이어 접촉 데미지(PlayerHealth가 읽음).</summary>
        public float ContactDamage => stats.damage;

        /// <summary>이동 속도(데이터). 이동 AI 모듈(<see cref="IMoveBehaviour"/>)이 읽음.</summary>
        public float MoveSpeed => stats.moveSpeed;

        /// <summary>아키타입(원거리/근거리 파생 소스). 빌더가 def.archetype 주입 — 유도 미사일 조준 타입 우선순위(M4-1)가 읽음.</summary>
        public Archetype Archetype { get; private set; }

        /// <summary>빌더가 조립 시 호출 — 아키타입 주입(조준 타입 분류용, M4-1).</summary>
        public void SetArchetype(Archetype archetype) => Archetype = archetype;

        // 공격 AI 모듈(M3-2)이 읽는 스탯. 데이터=stats, 전달 방식은 모듈이 결정.
        /// <summary>공격 데미지(탄/자폭). = 접촉과 동일 필드(stats.damage).</summary>
        public float Damage => stats.damage;
        /// <summary>발사 간격(초). 탄막/조준단발.</summary>
        public float FireInterval => stats.fireInterval;
        /// <summary>적 탄속. 탄막/조준단발.</summary>
        public float ProjectileSpeed => stats.projectileSpeed;
        /// <summary>한 번에 뿌리는 탄 수. 탄막.</summary>
        public int BarrageCount => stats.barrageCount;
        /// <summary>자폭 폭발 반경.</summary>
        public float SuicideRadius => stats.suicideRadius;

        // 이동 AI 모듈(M3-1)이 읽는 스탯.
        /// <summary>견제(Hover): 멈추는 선호 Z간격.</summary>
        public float HoverDistance => stats.hoverDistance;
        /// <summary>견제(Hover): 머무는 시간(초).</summary>
        public float HoverDuration => stats.hoverDuration;

        IMoveBehaviour _move;       // 빌더가 주입(M3-1). null이면 정지.
        IAttackBehaviour _attack;   // 빌더가 주입(M3-2). null이면 공격 없음.
        Action<Enemy> _return;
        Action<Vector3> _dropOnDeath;
        GameObject _visual;   // 주입된 비주얼 인스턴스(모델). 조립 시 부착·반납 시 파괴(M2-5c)
        float _despawnZ;
        float _hp;
        bool _dead;
        bool _attackSuppressed;   // 이동 모듈이 공격을 막는 프레임(예: Hover 접근/이탈 중엔 사격 정지, M4-7)

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선 + 체력/사망상태/드랍핸들러/공격억제 리셋.</summary>
        public void OnSpawned(Action<Enemy> returnToPool)
        {
            _return = returnToPool;
            _hp = stats.maxHp;
            _dead = false;
            _dropOnDeath = null;
            _attackSuppressed = false;   // 기본=공격 허용(비-Hover 적은 항상 사격). Hover만 매 프레임 갱신.
        }

        /// <summary>이동 모듈이 공격 허용/차단을 제어(M4-7 Hover) — true면 이 프레임 공격 틱 스킵. 매 프레임 이동 모듈이 세팅.</summary>
        public void SetAttackSuppressed(bool suppressed) => _attackSuppressed = suppressed;

        /// <summary>빌더가 스폰 시 호출 — effective 스탯(base×전역배율) 주입. 체력도 새 maxHp로 리셋(OnSpawned 이후 호출 대비).</summary>
        public void ApplyStats(EnemyStats effective)
        {
            stats = effective;
            _hp = effective.maxHp;
        }

        /// <summary>빌더가 조립 시 호출 — 캐시가 준 비주얼 프리팹을 셸 자식으로 부착(M2-5c). 이전 비주얼은 먼저 제거. null이면 무시.
        /// <paramref name="scale"/> = 비주얼 크기 배수(M3-3, 모델별 편차 보정) — 모델 원본 로컬스케일에 곱함. 히트박스(셸)는 불변.</summary>
        public void AttachVisual(GameObject prefab, float scale)
        {
            ClearVisual();
            if (prefab == null) return;
            _visual = Instantiate(prefab);
            _visual.transform.SetParent(transform, false);   // false=로컬 스케일 보존(셸 스케일에 곱)
            _visual.transform.localPosition = Vector3.zero;
            _visual.transform.localRotation = Quaternion.identity;
            if (scale > 0f && !Mathf.Approximately(scale, 1f))
                _visual.transform.localScale *= scale;   // 비주얼만 배수(히트박스=셸 고정)
        }

        /// <summary>반납 teardown — 부착된 비주얼 인스턴스 파괴(M2-5c). 프리팹(캐시)은 건드리지 않음.</summary>
        public void ClearVisual()
        {
            if (_visual != null) { Destroy(_visual); _visual = null; }
        }

        /// <summary>스포너가 스폰 시 호출 — 실사망 시 드랍할 콜백 주입(오브 드랍 M1-5). null이면 드랍 없음.</summary>
        public void SetDropHandler(Action<Vector3> dropOnDeath)
        {
            _dropOnDeath = dropOnDeath;
        }

        /// <summary>스포너가 스폰 시 호출 — despawn 기준 주입(월드 경계, 스포너 관심사). 이동 속도는 <see cref="stats"/>에서.</summary>
        public void Launch(float despawnZ)
        {
            _despawnZ = despawnZ;
        }

        /// <summary>빌더가 조립 시 호출(M3-1) — def.moveAI로 고른 이동 모듈 주입 + 스폰 상태 리셋. null이면 정지.</summary>
        public void SetMoveBehaviour(IMoveBehaviour move)
        {
            _move = move;
            _move?.OnSpawned();
        }

        /// <summary>빌더가 조립 시 호출(M3-2) — def.attackAI로 고른 공격 모듈 주입 + 스폰 상태 리셋. null이면 공격 없음.</summary>
        public void SetAttackBehaviour(IAttackBehaviour attack)
        {
            _attack = attack;
            _attack?.OnSpawned();
        }

        public void TakeDamage(float amount)
        {
            if (_dead) return;
            _hp -= amount;
            Debug.Log($"[TEMP] 적 피격 -{amount} → HP {_hp}/{stats.maxHp}", this);   // TODO: 임시 로그, 검증 후 제거
            if (_hp <= 0f) Die();
        }

        void Update()
        {
            float dt = Time.deltaTime;   // timeScale 0이면 0 → 자연 정지.

            // 이동·공격을 주입된 AI 모듈에 위임(M3-1/M3-2). 공격이 자폭으로 자멸시키면 _dead=true → 이후 스킵.
            _move?.Tick(this, dt);
            if (!_dead && !_attackSuppressed) _attack?.Tick(this, dt);   // Hover 접근/이탈 중엔 억제(멈춰있을 때만 사격)

            if (!_dead && transform.position.z <= _despawnZ) Despawn();
        }

        void Die()
        {
            Debug.Log("[TEMP] 적 사망 → 오브 드랍 + 처치점수 + 풀 반납", this);   // TODO: 임시 로그, 검증 후 제거
            // 실사망 위치에 오브 드랍(M1-5) + 처치 점수 발행(M1-9). 화면 밖 Despawn은 둘 다 안 함. 파괴 VFX(M4-9)는 이후.
            _dropOnDeath?.Invoke(transform.position);
            GameManager.Instance?.Events?.PublishEnemyKilled(stats.killScore);
            Despawn();
        }

        /// <summary>즉시 소멸(풀 반납). 드랍/처치점수 없음 — 화면 밖 이탈 및 자폭(<see cref="SuicideAttack"/>)이 사용.</summary>
        public void Despawn()
        {
            _dead = true;
            _return?.Invoke(this);
        }
    }
}
