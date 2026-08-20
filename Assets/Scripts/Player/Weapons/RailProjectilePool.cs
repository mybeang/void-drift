using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 레일건 투사체 전용 풀(M4-1 Step3) — <see cref="PooledObjectPool{T}"/> 상속. Get 시 반납 콜백 배선.
    /// (<see cref="ProjectilePool"/>·<see cref="HomingProjectilePool"/>과 동형 — 무기별 전용 풀·비주얼 분리.)
    /// </summary>
    public sealed class RailProjectilePool : PooledObjectPool<RailProjectile>
    {
        protected override void OnGet(RailProjectile item)
        {
            item.OnSpawned(Return);
        }
    }
}
