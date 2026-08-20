using UnityEngine;

namespace VD.Enemy
{
    /// <summary>
    /// 플레이어 위치 조회 공용 헬퍼(M3). <c>Player</c> 태그로 한 번 찾아 캐시 — 이동/공격 AI 모듈이 공유
    /// (Core→Player 타입 결합 회피, <see cref="OrbPool"/>과 동일 태그 방식). 플레이어는 1체라 전역 캐시로 충분.
    /// <para>Unity null 체크로 파괴(씬 재시작 등) 시 자동 재조회. static 캐시는 도메인 리로드(플레이 재진입)에 리셋.</para>
    /// </summary>
    public static class PlayerLocator
    {
        static Transform _cached;

        /// <summary>플레이어 Transform(없으면 null). 캐시가 살아있으면 재사용, 아니면 태그로 재조회.</summary>
        public static Transform Get()
        {
            if (_cached != null) return _cached;   // Unity null(파괴 시 false) 체크
            var go = GameObject.FindGameObjectWithTag("Player");
            _cached = go != null ? go.transform : null;
            return _cached;
        }
    }
}
