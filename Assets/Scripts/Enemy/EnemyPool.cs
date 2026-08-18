using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 전용 풀 — <see cref="PooledObjectPool{T}"/> 상속. Get 시 적에 반납 콜백을 배선한다.
    /// 위치·이동은 스포너(<see cref="EnemySpawner"/>)가 Get 후 세팅. (M1-4 1단계 신설)
    /// </summary>
    public sealed class EnemyPool : PooledObjectPool<Enemy>
    {
        protected override void OnGet(Enemy item)
        {
            item.OnSpawned(Return);
        }
    }
}
