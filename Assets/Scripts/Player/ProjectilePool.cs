using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 투사체 전용 풀 — <see cref="PooledObjectPool{T}"/> 상속. Get 시 투사체에 반납 콜백(<see cref="PooledObjectPool{T}.Return"/>)을 배선한다.
    /// 위치·회전은 발사기(<see cref="PlayerShooter"/>)가 Get 후 세팅. (M1-3 3단계 신설)
    /// </summary>
    public sealed class ProjectilePool : PooledObjectPool<Projectile>
    {
        protected override void OnGet(Projectile item)
        {
            item.OnSpawned(Return);
        }
    }
}
