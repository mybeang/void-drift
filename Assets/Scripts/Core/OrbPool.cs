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
        float _magnetBonus;   // 자석범위 강화 누적(M1-8) — 스폰 시 오브에 주입

        protected override void Awake()
        {
            base.Awake();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        protected override void OnGet(Orb item)
        {
            item.OnSpawned(_player, Return, _magnetBonus);
        }

        /// <summary>자석 반경 가산 강화 — M1-8. 이후 스폰되는 오브에 반영(가산 누적).</summary>
        public void AddMagnetRadius(float amount) => _magnetBonus += amount;
    }
}
