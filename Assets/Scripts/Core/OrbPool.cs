using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 오브 전용 풀 — <see cref="PooledObjectPool{T}"/> 상속. (M1-5)
    /// 자석 타깃(플레이어)을 태그 "Player"로 1회 탐색해 캐시하고, Get 시 오브에 타깃·반납 콜백을 배선한다.
    /// (Core→Player 타입 결합 회피를 위해 컴포넌트 타입이 아닌 태그로 참조.)
    /// 위치는 드랍 측(<see cref="Enemy"/> 사망 훅)이 Get 후 세팅.
    /// </summary>
    public sealed class OrbPool : PooledObjectPool<Orb>
    {
        Transform _player;

        protected override void Awake()
        {
            base.Awake();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        protected override void OnGet(Orb item)
        {
            item.OnSpawned(_player, Return);
        }
    }
}
