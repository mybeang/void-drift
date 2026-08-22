using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 사운드 설정 패널(M5-10, 밸런싱 패스에서 <c>SettingsPanel</c>→<c>SoundSettingsPanel</c> 개명).
    /// 마스터/BGM/SFX 볼륨 슬라이더 + 퍼센트 표기 + 닫기. 값은 <see cref="AudioManager"/>의 볼륨 API로 실시간 반영·PlayerPrefs 영속.
    /// 프리팹 1개를 <b>타이틀</b>(환경설정 버튼이 직접 오픈)·<b>인게임</b>(일시정지 메뉴 <see cref="SettingsPanel"/>의 "환경 설정"이 오픈) 양쪽에 배치.
    /// <para><see cref="pauseGameWhileOpen"/> = true면 열 때 <see cref="GameManager.Pause"/>, 닫을 때 <see cref="GameManager.Resume"/>.
    /// 인게임에선 상위 일시정지 메뉴가 정지를 소유하므로 <b>false</b>(닫으면 메뉴로 복귀). 타이틀도 false.</para>
    /// </summary>
    public sealed class SoundSettingsPanel : MonoBehaviour
    {
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider bgmSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] TMP_Text masterValue;
        [SerializeField] TMP_Text bgmValue;
        [SerializeField] TMP_Text sfxValue;
        [SerializeField] Button closeButton;

        [Tooltip("열려 있는 동안 게임 일시정지(인게임 인스턴스=true, 타이틀=false)")]
        [SerializeField] bool pauseGameWhileOpen;

        void Awake()
        {
            if (masterSlider) masterSlider.onValueChanged.AddListener(OnMaster);
            if (bgmSlider) bgmSlider.onValueChanged.AddListener(OnBgm);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfx);
            if (closeButton) closeButton.onClick.AddListener(Close);
        }

        /// <summary>패널 열기 — 슬라이더를 저장값으로 동기화 후 표시.</summary>
        public void Open()
        {
            gameObject.SetActive(true);       // Awake(리스너 배선) 보장
            transform.SetAsLastSibling();     // 최상단 표시

            var am = AudioManager.Instance;
            Sync(masterSlider, masterValue, am != null ? am.GetMasterVolume() : 1f);
            Sync(bgmSlider, bgmValue, am != null ? am.GetBgmVolume() : 1f);
            Sync(sfxSlider, sfxValue, am != null ? am.GetSfxVolume() : 1f);

            if (pauseGameWhileOpen) GameManager.Instance?.Pause();
        }

        /// <summary>패널 닫기 — 저장(디스크) 후 숨김.</summary>
        public void Close()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
            if (pauseGameWhileOpen) GameManager.Instance?.Resume();
        }

        void Sync(Slider s, TMP_Text t, float v)
        {
            if (s) s.SetValueWithoutNotify(v);   // 리스너 발화 없이 값만
            SetText(t, v);
        }

        void OnMaster(float v) { AudioManager.Instance?.SetMasterVolume(v); SetText(masterValue, v); }
        void OnBgm(float v) { AudioManager.Instance?.SetBgmVolume(v); SetText(bgmValue, v); }
        void OnSfx(float v) { AudioManager.Instance?.SetSfxVolume(v); SetText(sfxValue, v); }

        static void SetText(TMP_Text t, float v)
        {
            if (t) t.text = Mathf.RoundToInt(v * 100f) + "%";
        }
    }
}
