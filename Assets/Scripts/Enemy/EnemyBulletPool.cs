using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 탄환 전용 풀(M3-2) — <see cref="PooledObjectPool{T}"/> 상속. Get 시 탄환에 반납 콜백 배선.
    /// 위치·방향은 공격 모듈(<see cref="BarrageAttack"/> 등)이 Get 후 <see cref="EnemyBullet.Launch"/>로 세팅.
    /// (플레이어 <c>ProjectilePool</c>의 적 버전. 탄막이 한 번에 여러 발 → prewarm 넉넉히.)
    /// </summary>
    public sealed class EnemyBulletPool : PooledObjectPool<EnemyBullet>
    {
        protected override void OnGet(EnemyBullet item)
        {
            item.OnSpawned(Return);
        }
    }
}
