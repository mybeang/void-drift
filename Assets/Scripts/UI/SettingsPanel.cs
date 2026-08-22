using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 인게임 <b>일시정지 메뉴</b>(밸런싱 패스). 기어 버튼 또는 <b>ESC</b>로 여닫으며, 열려 있는 동안 게임 정지.
    /// 5버튼: <b>게임 재개 · 새로 시작 · 환경 설정 · 타이틀로 · 게임 종료</b>. "환경 설정"은 <see cref="SoundSettingsPanel"/>(사운드)을 띄운다.
    /// <para>루트는 <b>항상 활성</b>(ESC 감지 위해) — 표시/숨김은 <see cref="content"/> 자식만 토글한다.
    /// 씬 전환(새로 시작/타이틀)은 <see cref="SceneTransition"/> 이클립스 와이프.</para>
    /// 인게임 전용(타이틀은 SoundSettingsPanel을 직접 사용).
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [Header("표시 (루트는 항상 활성, 이 자식만 토글)")]
        [Tooltip("Dim+Window+버튼을 담은 컨테이너. 열림=활성, 닫힘=비활성.")]
        [SerializeField] GameObject content;

        [Header("열기 트리거")]
        [Tooltip("기어 버튼(선택). ESC도 동일 동작.")]
        [SerializeField] Button openButton;

        [Header("메뉴 버튼")]
        [SerializeField] Button resumeButton;   // 게임 재개
        [SerializeField] Button restartButton;  // 새로 시작
        [SerializeField] Button soundButton;     // 환경 설정
        [SerializeField] Button titleButton;     // 타이틀로
        [SerializeField] Button quitButton;      // 게임 종료

        [Header("참조")]
        [Tooltip("환경 설정 = 이 사운드 패널을 오픈")]
        [SerializeField] SoundSettingsPanel soundPanel;
        [SerializeField] string gameSceneName = "GameScene";
        [SerializeField] string titleSceneName = "TitleScene";

        bool IsOpen => content != null && content.activeSelf;

        void Awake()
        {
            if (openButton) openButton.onClick.AddListener(Open);
            if (resumeButton) resumeButton.onClick.AddListener(Close);
            if (restartButton) restartButton.onClick.AddListener(() => SceneTransition.Instance.TransitionTo(gameSceneName));
            if (soundButton) soundButton.onClick.AddListener(() => soundPanel?.Open());
            if (titleButton) titleButton.onClick.AddListener(() => SceneTransition.Instance.TransitionTo(titleSceneName));
            if (quitButton) quitButton.onClick.AddListener(Quit);
            if (content) content.SetActive(false);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

            if (IsOpen)
            {
                // 사운드 패널이 위에 떠 있으면 그걸 먼저 닫는다(메뉴는 유지).
                if (soundPanel != null && soundPanel.gameObject.activeSelf) soundPanel.Close();
                else Close();
            }
            else if (IsPlaying())
            {
                Open();
            }
        }

        /// <summary>메뉴 열기 — 표시 + 게임 일시정지.</summary>
        public void Open()
        {
            if (IsOpen || !IsPlaying()) return;
            if (content) { content.SetActive(true); transform.SetAsLastSibling(); }
            GameManager.Instance?.Pause();
        }

        /// <summary>메뉴 닫기(게임 재개) — 사운드 패널도 함께 닫고 정지 해제.</summary>
        public void Close()
        {
            if (soundPanel != null && soundPanel.gameObject.activeSelf) soundPanel.Close();
            if (content) content.SetActive(false);
            GameManager.Instance?.Resume();
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            return gm != null && gm.State == GameState.Playing;
        }
    }
}
