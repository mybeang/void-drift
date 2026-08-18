using System.Collections.Generic;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 공용 오브젝트 풀 <b>베이스(추상)</b>. 도메인별 풀이 <typeparamref name="T"/>를 지정해 상속한다.
    /// 예) <c>ProjectilePool : PooledObjectPool&lt;Projectile&gt;</c> (M1-4: EnemyPool, M1-5: OrbPool 등).
    /// 닫힌 서브클래스라 씬/프리팹에 컴포넌트로 부착·인스펙터 prewarm 가능.
    /// 공통 로직(prewarm/Get/Return/부모 정리)만 담고, 항목별 초기화는 하위(OnGet/OnReturn) 또는 항목이 담당해 <b>얇게</b> 유지.
    /// (M1-3 3단계 신설. 한 곳에 몰지 않고 상속으로 도메인별 분리 — 사용자 결정 2026-08-18.)
    /// </summary>
    public abstract class PooledObjectPool<T> : MonoBehaviour where T : Component
    {
        [Header("풀")]
        [Tooltip("풀링할 프리팹(컴포넌트 T를 가진 루트)")]
        [SerializeField] protected T prefab;
        [Tooltip("시작 시 미리 생성해둘 개수(런타임 Instantiate GC 스파이크 방지)")]
        [SerializeField] protected int prewarmCount = 16;

        readonly Queue<T> _idle = new Queue<T>();

        protected virtual void Awake()
        {
            for (int i = 0; i < prewarmCount; i++)
            {
                T item = Create();
                item.gameObject.SetActive(false);
                _idle.Enqueue(item);
            }
        }

        /// <summary>풀에서 하나 꺼내 활성화해 반환. 비었으면 새로 생성.</summary>
        public T Get()
        {
            T item = _idle.Count > 0 ? _idle.Dequeue() : Create();
            item.gameObject.SetActive(true);
            OnGet(item);
            return item;
        }

        /// <summary>사용 끝난 항목을 비활성화해 풀로 반납.</summary>
        public void Return(T item)
        {
            if (item == null) return;
            OnReturn(item);
            item.gameObject.SetActive(false);
            _idle.Enqueue(item);
        }

        /// <summary>인스턴스 생성. 기본은 prefab을 이 풀의 자식으로 Instantiate. 필요 시 하위가 override.</summary>
        protected virtual T Create()
        {
            return Instantiate(prefab, transform);
        }

        /// <summary>Get 직후 훅 — 항목별 초기화·콜백 배선. 기본 no-op.</summary>
        protected virtual void OnGet(T item) { }

        /// <summary>Return 직전 훅. 기본 no-op.</summary>
        protected virtual void OnReturn(T item) { }
    }
}
