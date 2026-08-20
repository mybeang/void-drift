using System;
using UnityEngine;
using VD.Player;

namespace VD.Enemy
{
    /// <summary>
    /// 적 탄환(M3-2). 발사 방향(=forward)으로 직진, 수명이 다하거나 플레이어를 히트하면 풀에 반납.
    /// 방향·탄속·수명·데미지는 공격 모듈(<see cref="BarrageAttack"/> 등)이 <see cref="Launch"/>로 주입.
    /// 히트 = <b>트리거 콜라이더</b>(프리팹에 kinematic Rigidbody + isTrigger). <c>EnemyBullet</c> 레이어라
    /// 물리 매트릭스상 <b>Player하고만</b> 충돌(적/아군탄 통과). 플레이어 <see cref="IDamageable"/> 미구현이라 직접 타격.
    /// (플레이어 <see cref="Projectile"/>의 미러 — 대상이 적이 아니라 플레이어인 점만 다름.)
    /// </summary>
    public sealed class EnemyBullet : MonoBehaviour
    {
        Action<EnemyBullet> _return;
        float _speed;
        float _lifetime;
        float _damage;
        float _age;
        bool _spent;

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선(풀의 책임).</summary>
        public void OnSpawned(Action<EnemyBullet> returnToPool)
        {
            _return = returnToPool;
        }

        /// <summary>공격 모듈이 발사 시 호출 — 진행 방향·탄속·수명·데미지 주입 + 상태 리셋.</summary>
        public void Launch(Vector3 direction, float speed, float lifetime, float damage)
        {
            if (direction.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(direction);   // forward = 진행 방향
            _speed = speed;
            _lifetime = lifetime;
            _damage = damage;
            _age = 0f;
            _spent = false;
        }

        void Update()
        {
            // timeScale 0(일시정지)이면 deltaTime 0 → 자연 정지.
            transform.position += transform.forward * (_speed * Time.deltaTime);

            _age += Time.deltaTime;
            if (!_spent && _age >= _lifetime) Despawn();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_spent) return;
            var ph = other.GetComponentInParent<PlayerHealth>();
            if (ph == null) return;   // 레이어상 Player만 오지만 방어적으로 확인

            ph.ApplyDamage(_damage);
            Despawn();
        }

        void Despawn()
        {
            _spent = true;
            _return?.Invoke(this);
        }
    }
}
