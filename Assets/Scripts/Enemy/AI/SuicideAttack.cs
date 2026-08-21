using UnityEngine;
using VD.Player;

namespace VD.Enemy
{
    /// <summary>
    /// 자폭 공격(M3-2 → I-6 상태화). 플레이어와 거리가 <see cref="Enemy.SuicideRadius"/> 이내로 들어오면
    /// <b>멈춰서 무장(Arm)</b>하고 <see cref="_armDuration"/>초 동안 빨간색으로 깜박이다가 <b>폭발</b>한다.
    /// 폭발 = <see cref="Enemy.PlaySuicideExplosion"/> VFX 재생 + <b>범위 감쇠 데미지</b>(중심=<see cref="Enemy.Damage"/> 최대,
    /// <see cref="_explosionRadius"/> 가장자리로 갈수록 <see cref="_minFactor"/>까지 감소, 반경 밖은 무피해) + 자멸(드랍/점수 없이 반납).
    /// <para>멈춤·깜박 연출은 셸(<see cref="Enemy.SetSuicideArming"/>)에 위임 — 이 모듈은 상태·타이밍만 관리.
    /// <b>쿨다운/페이즈 상태를 갖는 인스턴스별 모듈</b>(무상태 아님) → 빌더가 스폰마다 new. 대상은 플레이어 1체.</para>
    /// 수치(무장시간·폭발반경·최소비율)는 생성자 기본값=시작점, Day5 튜닝 / SO화 후보.
    /// </summary>
    public sealed class SuicideAttack : IAttackBehaviour
    {
        readonly float _armDuration;      // 무장(멈춤+깜박) 지속. 정의=1초
        readonly float _explosionRadius;  // 폭발 데미지 반경(작은 범위). 중심 최대 → 가장자리 _minFactor
        readonly float _minFactor;        // 가장자리 데미지 비율(정의=중심의 50%)

        enum Phase { Approach, Arming }
        Phase _phase;
        float _armTimer;

        public SuicideAttack(float armDuration = 1f, float explosionRadius = 6f, float minFactor = 0.5f)
        {
            _armDuration = armDuration;
            _explosionRadius = explosionRadius;
            _minFactor = minFactor;
        }

        public void OnSpawned()
        {
            _phase = Phase.Approach;
            _armTimer = 0f;
        }

        public void Tick(Enemy self, float dt)
        {
            if (_phase == Phase.Approach)
            {
                Transform target = PlayerLocator.Get();
                if (target == null) return;

                float r = self.SuicideRadius;
                if (r <= 0f) return;

                if ((target.position - self.transform.position).sqrMagnitude <= r * r)
                {
                    _phase = Phase.Arming;
                    _armTimer = _armDuration;
                    self.SetSuicideArming(true);   // 멈춤 + 빨간 깜박 개시
                }
            }
            else // Arming: 멈춰서 깜박이다 시간 다 되면 폭발
            {
                _armTimer -= dt;
                if (_armTimer <= 0f) Explode(self);
            }
        }

        void Explode(Enemy self)
        {
            self.SetSuicideArming(false);      // 무장 해제(깜박 중지)
            self.PlaySuicideExplosion();       // 폭발 VFX(멈춘 위치)

            // 범위 감쇠 데미지: 중심(dist=0)=Damage → 가장자리(dist=반경)=Damage×_minFactor. 반경 밖은 무피해.
            Transform target = PlayerLocator.Get();
            if (target != null && _explosionRadius > 0f)
            {
                float dist = (target.position - self.transform.position).magnitude;
                if (dist <= _explosionRadius)
                {
                    float factor = Mathf.Lerp(1f, _minFactor, dist / _explosionRadius);
                    target.GetComponentInParent<PlayerHealth>()?.ApplyDamage(self.Damage * factor);
                }
            }

            self.Despawn();   // 자멸(드랍/점수 없음)
        }
    }
}
