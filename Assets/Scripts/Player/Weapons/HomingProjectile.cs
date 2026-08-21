using System;
using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 유도 미사일 투사체(M4-1 Step2) — 발사 시 락한 타겟 Transform 쪽으로 <b>선회율 제한</b> 스티어링하며 전진, 단일 히트 소멸.
    /// 타겟이 죽거나(풀 반납으로 비활성) 사라지면 재조준하지 않고 <b>직진 관성</b>(빗나감 자연스러움, 사용자 결정).
    /// 히트 감지 = 트리거 콜라이더(<see cref="Projectile"/>와 동형: kinematic RB + isTrigger). 탄속·수명·데미지·선회율·타겟은 발사기가 <see cref="Launch"/>로 주입.
    /// </summary>
    public sealed class HomingProjectile : MonoBehaviour
    {
        [Tooltip("적 히트 시 재생할 VFX 프리팹(M4-9). CFXR 등 자가소멸 이펙트")]
        [SerializeField] GameObject hitVfx;

        Action<HomingProjectile> _return;
        Transform _target;
        float _speed;
        float _accel;      // 가속도(유닛/초^2)
        float _maxSpeed;   // 상한(0이하=무제한)
        float _lifetime;
        float _damage;
        float _turnRate;   // 도/초
        float _age;
        bool _spent;

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선(풀의 책임).</summary>
        public void OnSpawned(Action<HomingProjectile> returnToPool)
        {
            _return = returnToPool;
        }

        /// <summary>발사기가 발사 시 호출 — 초기탄속·가속도·상한·수명·데미지·선회율·타겟 주입 + 상태 리셋.</summary>
        public void Launch(float initialSpeed, float acceleration, float maxSpeed, float lifetime, float damage, float turnRate, Transform target)
        {
            _speed = initialSpeed;
            _accel = acceleration;
            _maxSpeed = maxSpeed;
            _lifetime = lifetime;
            _damage = damage;
            _turnRate = turnRate;
            _target = target;
            _age = 0f;
            _spent = false;
        }

        void Update()
        {
            if (_spent) return;

            // 타겟 살아있으면(활성) 그쪽으로 선회율만큼 회전. 죽었/사라졌으면 락 해제 → 현재 forward로 직진.
            if (_target != null && _target.gameObject.activeInHierarchy)
            {
                Vector3 toTarget = _target.position - transform.position;
                if (toTarget.sqrMagnitude > 1e-6f)
                {
                    Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, _turnRate * Time.deltaTime);
                }
            }
            else
            {
                _target = null;
            }

            // 가속도로 탄속 증가(상한 클램프). timeScale 0(일시정지)이면 deltaTime 0 → 자연 정지.
            _speed += _accel * Time.deltaTime;
            if (_maxSpeed > 0f && _speed > _maxSpeed) _speed = _maxSpeed;
            transform.position += transform.forward * (_speed * Time.deltaTime);

            _age += Time.deltaTime;
            if (_age >= _lifetime) Despawn();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_spent) return;
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;   // 적이 아님 → 무시

            target.TakeDamage(_damage);
            if (hitVfx != null) Instantiate(hitVfx, other.ClosestPoint(transform.position), Quaternion.identity);   // 히트 VFX(M4-9)
            Debug.Log($"[TEMP] 유도 히트 → {other.name} (dmg {_damage})", other);   // TODO: 임시 로그, 검증 후 제거
            Despawn();
        }

        void Despawn()
        {
            _spent = true;
            _target = null;
            _return?.Invoke(this);
        }
    }
}
