using System;
using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 투사체 — 자기 forward(= 발사 시점의 조준 축)로 직진, 수명이 다하거나 <see cref="IDamageable"/>를 히트하면 풀에 반납.
    /// 탄속·수명·데미지는 발사기(<see cref="PlayerShooter"/>)가 <see cref="Launch"/>로 주입(튜닝 한 곳).
    /// 히트 감지 = <b>트리거 콜라이더</b>(프리팹에 kinematic Rigidbody + isTrigger 콜라이더). 적만 데미지, 그 외(플레이어 등)는 무시.
    /// (M1-4: 데미지/히트 추가.)
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        Action<Projectile> _return;
        float _speed;
        float _lifetime;
        float _damage;
        float _age;
        bool _spent;

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선(풀의 책임).</summary>
        public void OnSpawned(Action<Projectile> returnToPool)
        {
            _return = returnToPool;
        }

        /// <summary>발사기가 발사 시 호출 — 탄속·수명·데미지 주입 + 상태 리셋.</summary>
        public void Launch(float speed, float lifetime, float damage)
        {
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
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;   // 적이 아님(플레이어·다른 투사체 등) → 무시

            target.TakeDamage(_damage);
            Debug.Log($"[TEMP] 투사체 히트 → {other.name} (dmg {_damage})", other);   // TODO: 임시 로그, 검증 후 제거
            Despawn();
        }

        void Despawn()
        {
            _spent = true;
            _return?.Invoke(this);
        }
    }
}
