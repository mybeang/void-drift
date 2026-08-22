using UnityEngine;
using UnityEngine.UI;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 조작법 튜토리얼 창(밸런싱 패스 ②). 게임 시작 시 HUD 위를 덮고, 열려 있는 동안
    /// <see cref="GameManager.CombatFrozen"/>=true로 <b>이동·사격·스폰을 정지</b>(<b>timeScale은 0 아님</b> — BGM/연출 유지).
    /// "시작하기"(닫기) → 전투 시작. "다시 보지 않기" 체크 시 <see cref="PlayerPrefs"/>에 저장해 다음부터 스킵.
    /// PC/모바일 조작 안내를 <b>플랫폼 감지</b>로 분기(<see cref="pcPanel"/>/<see cref="mobilePanel"/>).
    /// <para>디버그 키(P/G/R)는 안내하지 않는다 — 실제 조작(이동·실드·메뉴)만.</para>
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        const string PrefsKey = "vd_tutorial_hidden";

        [Header("표시 (이 컨테이너만 토글)")]
        [SerializeField] GameObject content;
        [SerializeField] Button closeButton;
        [Tooltip("체크 후 닫으면 다음부터 스킵(PlayerPrefs)")]
        [SerializeField] Toggle dontShowToggle;

        [Header("플랫폼별 안내 패널")]
        [SerializeField] GameObject pcPanel;
        [SerializeField] GameObject mobilePanel;

        [Header("테스트")]
        [Tooltip("에디터에서 스킵 무시하고 항상 표시")]
        [SerializeField] bool forceShowInEditor = true;

        void Awake()
        {
            if (closeButton) closeButton.onClick.AddListener(Close);
        }

        void Start()
        {
            bool skip = PlayerPrefs.GetInt(PrefsKey, 0) == 1;
#if UNITY_EDITOR
            if (forceShowInEditor) skip = false;
#endif
            if (skip) { if (content) content.SetActive(false); return; }

            // 플랫폼 분기(감지는 항상 결정적 → 한쪽만 표시).
            bool mobile = Application.isMobilePlatform;
            if (pcPanel) pcPanel.SetActive(!mobile);
            if (mobilePanel) mobilePanel.SetActive(mobile);

            if (content) { content.SetActive(true); content.transform.SetAsLastSibling(); }
            var gm = GameManager.Instance;
            if (gm != null) gm.CombatFrozen = true;   // 전투 보류(timeScale 불변)
        }

        /// <summary>닫기 = 전투 시작. "다시 보지 않기" 체크 시 스킵 저장.</summary>
        public void Close()
        {
            if (dontShowToggle != null && dontShowToggle.isOn)
            {
                PlayerPrefs.SetInt(PrefsKey, 1);
                PlayerPrefs.Save();
            }
            if (content) content.SetActive(false);
            var gm = GameManager.Instance;
            if (gm != null) gm.CombatFrozen = false;   // 전투 시작
        }
    }
}
