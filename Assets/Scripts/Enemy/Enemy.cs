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

        IMoveBehaviour _move;   // 빌더가 주입(M3-1). null이면 정지.
        Action<Enemy> _return;
        Action<Vector3> _dropOnDeath;
        GameObject _visual;   // 주입된 비주얼 인스턴스(모델). 조립 시 부착·반납 시 파괴(M2-5c)
        float _despawnZ;
        float _hp;
        bool _dead;

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선 + 체력/사망상태/드랍핸들러 리셋.</summary>
        public void OnSpawned(Action<Enemy> returnToPool)
        {
            _return = returnToPool;
            _hp = stats.maxHp;
            _dead = false;
            _dropOnDeath = null;
        }

        /// <summary>빌더가 스폰 시 호출 — effective 스탯(base×전역배율) 주입. 체력도 새 maxHp로 리셋(OnSpawned 이후 호출 대비).</summary>
        public void ApplyStats(EnemyStats effective)
        {
            stats = effective;
            _hp = effective.maxHp;
        }

        /// <summary>빌더가 조립 시 호출 — 캐시가 준 비주얼 프리팹을 셸 자식으로 부착(M2-5c). 이전 비주얼은 먼저 제거. null이면 무시.</summary>
        public void AttachVisual(GameObject prefab)
        {
            ClearVisual();
            if (prefab == null) return;
            _visual = Instantiate(prefab);
            _visual.transform.SetParent(transform, false);   // false=로컬 스케일 보존(셸 스케일에 곱)
            _visual.transform.localPosition = Vector3.zero;
            _visual.transform.localRotation = Quaternion.identity;
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

        public void TakeDamage(float amount)
        {
            if (_dead) return;
            _hp -= amount;
            Debug.Log($"[TEMP] 적 피격 -{amount} → HP {_hp}/{stats.maxHp}", this);   // TODO: 임시 로그, 검증 후 제거
            if (_hp <= 0f) Die();
        }

        void Update()
        {
            // 이동은 주입된 AI 모듈에 위임(M3-1). timeScale 0이면 dt=0 → 자연 정지. 속도 = 데이터(stats).
            _move?.Tick(this, Time.deltaTime);

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

        void Despawn()
        {
            _dead = true;
            _return?.Invoke(this);
        }
    }
}
