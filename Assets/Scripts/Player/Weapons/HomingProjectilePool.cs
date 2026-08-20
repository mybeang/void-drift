using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 유도 미사일 전용 풀(M4-1 Step2) — <see cref="PooledObjectPool{T}"/> 상속. Get 시 반납 콜백 배선.
    /// (<see cref="ProjectilePool"/>과 동형 — 무기별 전용 풀·비주얼 분리.)
    /// </summary>
    public sealed class HomingProjectilePool : PooledObjectPool<HomingProjectile>
    {
        protected override void OnGet(HomingProjectile item)
        {
            item.OnSpawned(Return);
        }
    }
}
