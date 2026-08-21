using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 최고점 "DB 역할" 접근 객체 SO(M4-10). GameScene(커밋)·ResultScene(표시) 양쪽이
    /// 인스펙터로 <b>같은 에셋</b>을 참조해 공유한다. 실제 영속화는 <see cref="IHighScoreStore"/>
    /// (현재 <see cref="LocalObscureStore"/> = 로컬 암호화 파일)에 위임하며,
    /// 후일 Firebase 리더보드(M5-7)는 <b>store 구현체만 교체</b>하면 된다(이 SO·상위 코드 무변경).
    /// <para><see cref="LastScore"/>는 인메모리 — SO 에셋이 한 플레이 세션 동안 로드된 채 유지되어
    /// 씬 전환(GameScene→ResultScene) 간 살아있으므로, 현재 판 점수 전달에 PlayerPrefs가 필요 없다.
    /// <see cref="Best"/>는 최초 접근 시 store에서 로드하고 신기록 시 갱신·영속화한다.</para>
    /// <para>⚠️ ScriptableObject 값 자체는 <b>빌드에서 영속되지 않는다</b>(앱 재실행 시 리셋).
    /// 이 SO는 접근 지점일 뿐이며, 실제 저장은 반드시 store가 담당한다.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "HighScoreRepository", menuName = "Void Drift/High Score Repository")]
    public sealed class HighScoreRepository : ScriptableObject
    {
        int _lastScore;
        int _best = -1;            // -1 = 아직 store에서 미로드
        bool _lastWasRecord;
        IHighScoreStore _store;

        IHighScoreStore Store => _store ??= new LocalObscureStore();

        /// <summary>직전 판(런) 최종 점수 — GameOver에서 <see cref="Commit"/>로 세팅, ResultScene이 읽는다.</summary>
        public int LastScore => _lastScore;

        /// <summary>직전 <see cref="Commit"/>가 신기록이었는지 — ResultScene "신기록!" 연출용.</summary>
        public bool LastWasRecord => _lastWasRecord;

        /// <summary>저장된 최고점. 최초 접근 시 store에서 로드(이후 캐시).</summary>
        public int Best
        {
            get
            {
                if (_best < 0) _best = Store.LoadBest();
                return _best;
            }
        }

        /// <summary>
        /// 한 판 종료(GameOver) 시 호출. 현재 점수를 <see cref="LastScore"/>로 기록하고,
        /// 최고점을 넘으면 갱신·영속화한다.
        /// </summary>
        /// <returns>신기록이면 true(ResultScene "신기록!" 연출용).</returns>
        public bool Commit(int score)
        {
            if (score < 0) score = 0;
            _lastScore = score;

            if (score > Best)
            {
                _best = score;
                Store.SaveBest(score);
                _lastWasRecord = true;
                return true;
            }
            _lastWasRecord = false;
            return false;
        }

        /// <summary>저장된 최고점을 0으로 초기화(테스트/디버그용).</summary>
        public void ResetBest()
        {
            _best = 0;
            Store.SaveBest(0);
        }

        // 에디터 도메인 리로드/에셋 최초 로드 시 인메모리 캐시를 초기화 — 이전 세션 잔여값 방지.
        // (씬 전환으로는 호출되지 않으므로 LastScore의 씬 간 전달을 깨지 않는다.)
        void OnEnable()
        {
            _lastScore = 0;
            _best = -1;
            _lastWasRecord = false;
            _store = null;
        }
    }
}
