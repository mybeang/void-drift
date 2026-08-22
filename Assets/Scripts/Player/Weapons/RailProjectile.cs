using System;
using System.Collections.Generic;
using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 레일건 투사체(M4-1 Step3) — 초고속 직진, 첫 히트에 소멸하지 않고 <b>최대 관통 수(maxPierce)</b>만큼 일렬로 뚫으며
    /// 각 적에 데미지. 관통할수록 데미지 감소(damageDecay 곱, weapon-acquisition §3 · Lv5 관통효율 완화는 M5-4).
    /// 같은 적 중복 히트 방지(관통 중 콜라이더 재진입 무시). 궤적 = 프리팹의 TrailRenderer(비주얼).
    /// 히트 감지 = 트리거 콜라이더(<see cref="Projectile"/> 동형). 수치는 발사기가 <see cref="Launch"/>로 주입.
    /// </summary>
    public sealed class RailProjectile : MonoBehaviour
    {
        [Tooltip("적 히트 시 재생할 VFX 프리팹(M4-9, 관통 히트마다). CFXR 등 자가소멸 이펙트")]
        [SerializeField] GameObject hitVfx;

        Action<RailProjectile> _return;
        float _speed;
        float _lifetime;
        float _damage;
        float _damageDecay;   // 관통당 데미지 곱(예 0.7 = -30%)
        int _maxPierce;
        int _hitCount;
        float _age;
        bool _spent;
        readonly HashSet<int> _hit = new HashSet<int>();   // 중복 히트 방지(관통 대상 인스턴스ID)
        TrailRenderer _trail;

        void Awake()
        {
            _trail = GetComponentInChildren<TrailRenderer>(true);   // 궤적 VFX(있으면). 풀 재사용 시 잔상 제거용 캐시.
        }

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선(풀의 책임).</summary>
        public void OnSpawned(Action<RailProjectile> returnToPool)
        {
            _return = returnToPool;
        }

        /// <summary>발사기가 발사 시 호출 — 탄속·수명·데미지·관통감쇠·최대관통 주입 + 상태 리셋.</summary>
        public void Launch(float speed, float lifetime, float damage, float damageDecay, int maxPierce)
        {
            _speed = speed;
            _lifetime = lifetime;
            _damage = damage;
            _damageDecay = damageDecay;
            _maxPierce = Mathf.Max(1, maxPierce);
            _hitCount = 0;
            _age = 0f;
            _spent = false;
            _hit.Clear();
            if (_trail != null) _trail.Clear();   // 풀 재사용 잔상 제거(새 위치로 옮긴 뒤 호출됨)
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
            var dmg = other.GetComponentInParent<IDamageable>();
            if (dmg == null) return;   // 적이 아님 → 무시(관통 계속)

            // 같은 적 중복 히트 방지(관통 중 여러 콜라이더/재진입).
            var comp = dmg as Component;
            int id = comp != null ? comp.GetInstanceID() : other.GetInstanceID();
            if (!_hit.Add(id)) return;

            dmg.TakeDamage(_damage);
            Vector3 hitPos = other.ClosestPoint(transform.position);
            if (hitVfx != null) Instantiate(hitVfx, hitPos, Quaternion.identity);   // 히트 VFX(M4-9)
            AudioManager.Instance?.PlaySfx(SfxId.RailgunHit, hitPos);   // 명중음(M5-5, 관통마다)
            Debug.Log($"[TEMP] 레일 관통 히트 → {other.name} (dmg {_damage:F1}, {_hitCount + 1}/{_maxPierce})", other);   // TODO: 임시 로그, 검증 후 제거
            _hitCount++;
            _damage *= _damageDecay;   // 관통할수록 감쇠
            if (_hitCount >= _maxPierce) Despawn();
        }

        void Despawn()
        {
            _spent = true;
            _return?.Invoke(this);
        }
    }
}
