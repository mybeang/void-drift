using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 인게임 HUD (M1-10, uGUI+TMP). <see cref="GameEvents"/> 구독 → 생존시간/점수/HP/레벨/경험치 실시간 표시.
    /// 우측 상단 스택(레이아웃은 "최소 투자" 방침, Day5 조정). 표시 전용(입력 없음).
    /// 상태는 전부 GameEvents에서 옴(진행=05 문서). 참조는 씬에서 배선.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] TMP_Text timeText;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] Image hpFill;
        [SerializeField] Image xpFill;
        [Tooltip("HP 숫자 표기(남은/총). M3-4 HUD 개선")]
        [SerializeField] TMP_Text hpValueText;

        void Start()
        {
            var events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (events == null)
            {
                Debug.LogWarning("[HudView] GameManager/Events 없음 — 비활성.", this);
                enabled = false;
                return;
            }

            // ReadOnlyReactiveProperty는 구독 즉시 현재값 방출 → 초기 표시도 세팅됨.
            events.SurvivalTime.Subscribe(t => { if (timeText) timeText.text = FormatTime(t); }).AddTo(this);
            events.Score.Subscribe(s => { if (scoreText) scoreText.text = "SCORE  " + s; }).AddTo(this);
            events.Level.Subscribe(l => { if (levelText) levelText.text = "Lv " + l; }).AddTo(this);
            events.HpNormalized.Subscribe(h => { if (hpFill) hpFill.fillAmount = h; }).AddTo(this);
            events.HpValues.Subscribe(v => { if (hpValueText) hpValueText.text = Mathf.CeilToInt(v.Current) + "/" + Mathf.CeilToInt(v.Max); }).AddTo(this);
            events.XpNormalized.Subscribe(x => { if (xpFill) xpFill.fillAmount = x; }).AddTo(this);
        }

        static string FormatTime(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            return string.Format("{0:00}:{1:00}", s / 60, s % 60);
        }
    }
}
