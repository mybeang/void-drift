using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 견제(호버) 이동(M4-7, enemy-design §3) — 선호 Z거리까지 -Z로 접근한 뒤 그 거리에서 <b>일정 시간 머물며 사격</b>,
    /// 시간이 지나면 다시 -Z로 접근해 지나간다(<b>despawn 보장</b>, 사용자 결정 2026-08-21). AimedShot/Barrage 원거리 공격과 짝.
    /// <b>사격은 머무는(Hover) 동안만</b> — 접근/이탈 중엔 <see cref="Enemy.SetAttackSuppressed"/>(true)로 사격 억제.
    /// <para>3상태(Approach→Hover→Leave)가 per-instance 상태 → 빌더가 스폰마다 new(싱글톤 공유 금지), <see cref="OnSpawned"/>에서 리셋.
    /// 거리 기준 = 플레이어와의 <b>Z 간격</b>(코스축). 머무는 동안엔 위치 고정(측면 추적 없음, v1).
    /// <b>선호 거리·머무는 시간은 SO 데이터</b>(<see cref="Enemy.HoverDistance"/>/<see cref="Enemy.HoverDuration"/>, 오서링 툴 편집, M4-7) — MoveSpeed와 동일하게 self에서 읽음.</para>
    /// </summary>
    public sealed class HoverMove : IMoveBehaviour
    {
        enum Phase { Approach, Hover, Leave }

        Phase _phase;
        float _hoverTimer;

        public void OnSpawned()
        {
            _phase = Phase.Approach;
            _hoverTimer = 0f;
        }

        public void Tick(Enemy self, float dt)
        {
            Vector3 pos = self.transform.position;
            float step = self.MoveSpeed * dt;

            switch (_phase)
            {
                case Phase.Approach:
                    pos += Vector3.back * step;   // -Z 접근
                    Transform target = PlayerLocator.Get();
                    // 선호 Z간격 이내(플레이어 앞)로 들어오면 머묾. 타깃 없으면 계속 접근 → 결국 despawn.
                    if (target != null && (pos.z - target.position.z) <= self.HoverDistance)
                        _phase = Phase.Hover;
                    break;

                case Phase.Hover:
                    // 위치 고정(거리 유지). 머무는 시간이 차면 이탈로 전환.
                    _hoverTimer += dt;
                    if (_hoverTimer >= self.HoverDuration) _phase = Phase.Leave;
                    break;

                case Phase.Leave:
                    pos += Vector3.back * step;   // 다시 -Z → despawn 경계(_despawnZ) 도달
                    break;
            }

            self.transform.position = pos;

            // 사격은 '멈춰있는 동안(Hover)'만 — 접근/이탈 중엔 억제(사용자 결정 2026-08-21).
            self.SetAttackSuppressed(_phase != Phase.Hover);
        }
    }
}
