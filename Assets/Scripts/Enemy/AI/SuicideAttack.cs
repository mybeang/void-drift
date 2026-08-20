using UnityEngine;
using VD.Player;

namespace VD.Enemy
{
    /// <summary>
    /// 자폭 공격(M3-2). 플레이어와 거리가 <see cref="Enemy.SuicideRadius"/> 이내로 들어오면 폭발 —
    /// 플레이어에 <see cref="Enemy.Damage"/>를 주고 자기 자신은 즉시 소멸(드랍/처치점수 없음, 자멸이라).
    /// 돌진형(충돌=단발 접촉)과의 차별점 = <b>범위 트리거</b>(접촉 전 일정 반경에서 터짐). 폭발 VFX는 M4-9.
    /// 대상이 플레이어 1체뿐이라 실질 단일 타깃. 무상태 → 싱글톤 공유. 근접까지의 이동은 이동 AI 담당(추적 권장).
    /// </summary>
    public sealed class SuicideAttack : IAttackBehaviour
    {
        public void OnSpawned() { }

        public void Tick(Enemy self, float dt)
        {
            Transform target = PlayerLocator.Get();
            if (target == null) return;

            float r = self.SuicideRadius;
            if (r <= 0f) return;

            if ((target.position - self.transform.position).sqrMagnitude <= r * r)
            {
                // 폭발: 플레이어 데미지 + 자멸(드랍/점수 없이 반납)
                target.GetComponentInParent<PlayerHealth>()?.ApplyDamage(self.Damage);
                self.Despawn();
            }
        }
    }
}
