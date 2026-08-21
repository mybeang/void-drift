namespace VD.Core
{
    /// <summary>
    /// 최고점 영속 저장소 추상화(M4-10). 지금은 로컬(암호화 파일) 구현이 쓰이고,
    /// 나중에 Firebase 리더보드(M5-7) 도입 시 <b>이 인터페이스의 구현체만 교체</b>하면
    /// 상위 코드(<see cref="HighScoreRepository"/>·GameOver 커밋·ResultScene 표시)는 무변경이다.
    /// <para>동기 API — 로컬 파일용. Firebase는 네트워크라 비동기 확장이 필요하지만,
    /// 그 래핑/추가 시그니처는 M5-7에서 결정한다(지금 로컬만 만족하면 충분).</para>
    /// </summary>
    public interface IHighScoreStore
    {
        /// <summary>저장된 최고점을 읽는다. 없거나 손상/변조된 경우 0을 반환(안전 폴백).</summary>
        int LoadBest();

        /// <summary>최고점을 저장(덮어쓰기)한다.</summary>
        void SaveBest(int score);
    }
}
