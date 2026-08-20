using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 사행 이동(M3-3, 원래 M4-7 → 선반영). -Z로 접근하면서 <b>좌우(X) 사인파</b>로 흔든다(enemy-design §3).
    /// 진폭·주기는 Day5/SO화 후보. 위상 누적(`_t`)이 <b>per-instance 상태</b> → 빌더가 스폰마다 new(싱글톤 공유 금지).
    /// <para>중심 X를 저장하지 않으려 <b>측면 속도</b>(사인의 미분=코사인)로 적분 → 스폰 X 기준으로 자연 진동.
    /// -Z는 항상 진행하므로 despawn 보장(직진/추적과 동일).</para>
    /// </summary>
    public sealed class WeaveMove : IMoveBehaviour
    {
        readonly float _amplitude;   // 좌우 진폭(월드). Day5 튜닝 / SO화 후보
        readonly float _frequency;   // 흔들림 각주파수(rad/s). Day5 튜닝 / SO화 후보
        float _t;                    // 위상 누적(per-instance)

        public WeaveMove(float amplitude = 4f, float frequency = 3f)
        {
            _amplitude = amplitude;
            _frequency = frequency;
        }

        public void OnSpawned()
        {
            _t = 0f;
        }

        public void Tick(Enemy self, float dt)
        {
            _t += dt;
            Vector3 pos = self.transform.position;

            pos += Vector3.back * (self.MoveSpeed * dt);   // -Z 접근(despawn 보장)

            // x(t) = 스폰X + amp*sin(freq*t) 를 위치 저장 없이: 측면속도 = d/dt = amp*freq*cos(freq*t)
            float lateralV = _amplitude * _frequency * Mathf.Cos(_frequency * _t);
            pos.x += lateralV * dt;

            self.transform.position = pos;
        }
    }
}
