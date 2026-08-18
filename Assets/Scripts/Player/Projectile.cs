using System;
using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 투사체 — 자기 forward(= 발사 시점의 조준 축)로 직진하고, 수명이 다하면 스스로 풀에 반납한다.
    /// 운동 파라미터(탄속·수명)는 <b>보유하지 않고</b> 발사기(<see cref="PlayerShooter"/>)가 <see cref="Launch"/>로 주입한다
    /// (튜닝을 발사기 한 곳에 모음 — 사용자 결정 2026-08-18). 이번 단계는 이동·수명만 담당(비주얼 = 임시 프리미티브).
    /// 충돌 감지·데미지(<c>IDamageable</c>)는 적 엔티티가 생기는 M1-4에서 추가. (M1-3 3단계 신설)
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        Action<Projectile> _return;
        float _speed;
        float _lifetime;
        float _age;

        /// <summary>풀이 Get 시 호출 — 반납 콜백 배선(풀의 책임). 위치·회전·운동은 발사기가 <see cref="Launch"/>로.</summary>
        public void OnSpawned(Action<Projectile> returnToPool)
        {
            _return = returnToPool;
        }

        /// <summary>발사기가 발사 시 호출 — 탄속·수명 주입 + 수명 타이머 리셋.</summary>
        public void Launch(float speed, float lifetime)
        {
            _speed = speed;
            _lifetime = lifetime;
            _age = 0f;
        }

        void Update()
        {
            // timeScale 0(일시정지)이면 deltaTime 0 → 자연히 정지.
            transform.position += transform.forward * (_speed * Time.deltaTime);

            _age += Time.deltaTime;
            if (_age >= _lifetime)
                _return?.Invoke(this);
        }
    }
}
