using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 타이틀 씬(M5-9) 컨트롤러. 최고 점수를 표시하고 [게임 시작]·[게임 종료] 버튼을 배선한다.
    /// 시작 = 이클립스 와이프(<see cref="SceneTransition"/>)로 GameScene 전환 → 게임 루프
    /// (타이틀→게임→결과→타이틀)의 진입점. 결과 화면의 [타이틀]이 이 씬으로 복귀한다.
    /// </summary>
    public sealed class TitleController : MonoBehaviour
    {
        [SerializeField] HighScoreRepository highScore;
        [Tooltip("최고 점수 '값'만 표시(라벨 'BEST'는 별도 텍스트)")]
        [SerializeField] TMP_Text bestValue;
        [SerializeField] Button startButton;
        [SerializeField] Button quitButton;
        [SerializeField] string gameSceneName = "GameScene";

        void Start()
        {
            Time.timeScale = 1f; // 이전 씬(게임오버 프리즈 등) 잔재 복구

            int best = highScore != null ? highScore.Best : 0;
            if (bestValue) bestValue.text = best.ToString();

            if (startButton) startButton.onClick.AddListener(OnStart);
            if (quitButton) quitButton.onClick.AddListener(OnQuit);
        }

        void OnStart()
        {
            SceneTransition.Instance.TransitionTo(gameSceneName);
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
