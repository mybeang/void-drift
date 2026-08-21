using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 결과 화면(M4-10, ResultScene). 공유 <see cref="HighScoreRepository"/>에서 직전 판 점수·최고점을 읽어 표시하고,
    /// 신기록이면 라벨을 켠다. [다시하기]=GameScene 재로드 · [타이틀]=TitleScene — 둘 다 이클립스 와이프 전환.
    /// GameScene과 <b>같은 HighScoreRepository 에셋</b>을 참조해야 점수가 넘어온다(SO 인메모리 전달).
    /// </summary>
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] HighScoreRepository highScore;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text bestText;
        [Tooltip("신기록일 때만 켜지는 라벨(선택)")]
        [SerializeField] GameObject newRecordLabel;
        [SerializeField] Button retryButton;
        [SerializeField] Button titleButton;
        [SerializeField] string gameSceneName = "GameScene";
        [SerializeField] string titleSceneName = "TitleScene";

        void Start()
        {
            Time.timeScale = 1f; // 게임오버 프리즈 잔재 복구(안전)

            int score = highScore != null ? highScore.LastScore : 0;
            int best = highScore != null ? highScore.Best : 0;
            bool isRecord = highScore != null && highScore.LastWasRecord;

            if (scoreText) scoreText.text = "SCORE  " + score;
            if (bestText) bestText.text = "BEST  " + best;
            if (newRecordLabel) newRecordLabel.SetActive(isRecord);

            if (retryButton) retryButton.onClick.AddListener(() => SceneTransition.Instance.TransitionTo(gameSceneName));
            if (titleButton) titleButton.onClick.AddListener(() => SceneTransition.Instance.TransitionTo(titleSceneName));
        }
    }
}
