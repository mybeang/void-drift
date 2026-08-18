using R3;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 점수/생존시간 집계 (M1-9). 점수 = <b>생존시간 + 적 처치별 점수</b>(progression §3).
    /// 생존시간은 <see cref="GameState.Playing"/> 동안만 누적(일시정지/게임오버 시 정지),
    /// 처치 점수는 <see cref="GameEvents.EnemyKilled"/> 구독 합산. 결과는 <see cref="GameEvents"/>에 게시(HUD M1-10·결과 구독).
    /// 게임오버 시 최종값을 임시 로그(결과 UI/ResultScene은 이후). 수치(초당 점수)는 Day5. GameScene에 1개.
    /// </summary>
    public sealed class ScoreSystem : MonoBehaviour
    {
        [Tooltip("생존 1초당 점수. 수치 Day5")]
        [SerializeField] float timeScoreRate = 1f;

        GameEvents _events;
        float _survivalTime;
        int _killScore;

        void Start()
        {
            _events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (_events == null)
            {
                Debug.LogWarning("[ScoreSystem] GameManager/Events 없음 — 비활성.", this);
                enabled = false;
                return;
            }

            _events.EnemyKilled.Subscribe(OnEnemyKilled).AddTo(this);
            _events.State.Subscribe(OnState).AddTo(this);
            Push();
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            _survivalTime += Time.deltaTime;   // timeScale 0(정지/게임오버)엔 어차피 0
            Push();
        }

        void OnEnemyKilled(int score)
        {
            _killScore += score;
            Push();
        }

        void OnState(GameState state)
        {
            if (state == GameState.GameOver)
                Debug.Log($"[TEMP] 게임오버 — 생존 {_survivalTime:0.0}s / 점수 {CurrentScore()} (처치점수 {_killScore})", this);
        }

        int CurrentScore() => Mathf.RoundToInt(_survivalTime * timeScoreRate) + _killScore;

        void Push()
        {
            _events.SetSurvivalTime(_survivalTime);
            _events.SetScore(CurrentScore());
        }
    }
}
