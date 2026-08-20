using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 추적 이동(M3-1). 스폰 직후엔 직진(-Z)하다가 <b>플레이어와 거리가 <see cref="_homingRange"/> 이내</b>로
    /// 들어오면 그때부터 XY를 플레이어 쪽으로 보정한다(먼 거리 추적 X — 근접 시 달려듦). -Z는 항상 진행 → despawn 보장.
    /// 플레이어는 <c>Player</c> 태그로 조회(<see cref="OrbPool"/>과 동일 방식, Core→Player 결합 회피).
    /// 상태는 임계값(config)과 전역 타깃(플레이어 1체)뿐 → 무상태로 간주, 빌더가 싱글톤 공유 가능.
    /// <para>보정량은 <see cref="Enemy.MoveSpeed"/> 재사용(오버슛 방지 클램프). 임계·조향 수치는 Day5 튜닝 / SO화 후보.</para>
    /// </summary>
    public sealed class ChaseMove : IMoveBehaviour
    {
        readonly float _homingRange;   // 이 거리 이내로 들어오면 추적 개시(Day5 튜닝 / SO화 후보)

        public ChaseMove(float homingRange = 30f)
        {
            _homingRange = homingRange;
        }

        public void OnSpawned() { }

        public void Tick(Enemy self, float dt)
        {
            Vector3 pos = self.transform.position;
            float step = self.MoveSpeed * dt;

            pos += Vector3.back * step;   // 항상 -Z 접근(직진과 동일 → 반드시 despawn 경계 도달)

            Transform target = PlayerLocator.Get();
            if (target != null)
            {
                Vector3 tp = target.position;
                // 임계 거리 이내일 때만 XY 보정(밖이면 직진 유지). step 이내 클램프로 오버슛/떨림 방지.
                if ((tp - pos).sqrMagnitude <= _homingRange * _homingRange)
                {
                    Vector2 toXY = new Vector2(tp.x - pos.x, tp.y - pos.y);
                    Vector2 stepXY = Vector2.ClampMagnitude(toXY, step);
                    pos.x += stepXY.x;
                    pos.y += stepXY.y;
                }
            }

            self.transform.position = pos;
        }
    }
}
