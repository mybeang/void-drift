using System;
using UnityEngine;
using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 엔티티 (하드코딩 M1-4). 직진 접근 이동(-Z) + <see cref="IDamageable"/>(투사체 피격→HP 감소→사망).
    /// 사망·despawn 모두 풀에 반납한다(오브 드랍 M1-5·파괴 VFX M4-9는 이후). 플레이어 충돌 데미지는 M1-9.
    /// 이동 속도·despawn 기준은 스포너가 <see cref="Launch"/>로 주입. HP는 여기 단일 하드코딩(M2-2에서 SO화).
    /// M3-1에서 이동을 AI 모듈(직진/추적)로 분리 예정 — 지금은 직진 하드코딩.
    /// </summary>
    public sealed class Enemy : MonoBehaviour, IDamageable
    {
        [Tooltip("최대 체력. 스폰 시 이 값으로 리셋. 수치는 Day5(→ M2-2 SO)")]
        [SerializeField] float maxHp = 30f;
        [Tooltip("플레이어 접촉 시 주는 데미지. 수치는 Day5(→ M2-2 SO)")]
        [SerializeField] float contactDamage = 10f;
        [Tooltip("실사망(처치) 시 주는 점수. 수치는 Day5(→ M2-2 SO 적 테이블)")]
        [SerializeField] int killScore = 10;

        /// <summary>플레이어 접촉 데미지(PlayerHealth가 읽음).</summary>
        public float ContactDamage => contactDamage;

        Action<Enemy> _return;
        Action<Vector3> _dropOnDeath;
        float _speed;
        float _despawnZ;
        float _hp;
        bool _dead;

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선 + 체력/사망상태/드랍핸들러 리셋.</summary>
        public void OnSpawned(Action<Enemy> returnToPool)
        {
            _return = returnToPool;
            _hp = maxHp;
            _dead = false;
            _dropOnDeath = null;
        }

        /// <summary>스포너가 스폰 시 호출 — 실사망 시 드랍할 콜백 주입(오브 드랍 M1-5). null이면 드랍 없음.</summary>
        public void SetDropHandler(Action<Vector3> dropOnDeath)
        {
            _dropOnDeath = dropOnDeath;
        }

        /// <summary>스포너가 스폰 시 호출 — 이동 속도·despawn 기준 주입.</summary>
        public void Launch(float speed, float despawnZ)
        {
            _speed = speed;
            _despawnZ = despawnZ;
        }

        public void TakeDamage(float amount)
        {
            if (_dead) return;
            _hp -= amount;
            Debug.Log($"[TEMP] 적 피격 -{amount} → HP {_hp}/{maxHp}", this);   // TODO: 임시 로그, 검증 후 제거
            if (_hp <= 0f) Die();
        }

        void Update()
        {
            // 직진 접근: 월드 -Z로 이동(플레이어/카메라 쪽). timeScale 0이면 자연 정지.
            transform.position += Vector3.back * (_speed * Time.deltaTime);

            if (!_dead && transform.position.z <= _despawnZ) Despawn();
        }

        void Die()
        {
            Debug.Log("[TEMP] 적 사망 → 오브 드랍 + 처치점수 + 풀 반납", this);   // TODO: 임시 로그, 검증 후 제거
            // 실사망 위치에 오브 드랍(M1-5) + 처치 점수 발행(M1-9). 화면 밖 Despawn은 둘 다 안 함. 파괴 VFX(M4-9)는 이후.
            _dropOnDeath?.Invoke(transform.position);
            GameManager.Instance?.Events?.PublishEnemyKilled(killScore);
            Despawn();
        }

        void Despawn()
        {
            _dead = true;
            _return?.Invoke(this);
        }
    }
}
