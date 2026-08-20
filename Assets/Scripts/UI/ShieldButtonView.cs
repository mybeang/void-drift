using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VD.Core;
using VD.Player;

namespace VD.UI
{
    /// <summary>
    /// 실드 코너 버튼 (M4-4). 탭 → <see cref="PlayerShield.TryActivate"/>(Space와 동일 발동).
    /// <see cref="GameEvents.Shield"/> 구독 → 준비 시 interactable, 쿨다운 중 <see cref="cooldownFill"/>(Filled)로 남은 비율 표시.
    /// HudView(표시 전용)와 달리 입력을 받으므로 별도 컴포넌트로 분리.
    /// </summary>
    public sealed class ShieldButtonView : MonoBehaviour
    {
        [Tooltip("실드 버튼(탭 발동)")]
        [SerializeField] Button button;
        [Tooltip("쿨다운 오버레이(Image type=Filled). fillAmount=남은 쿨다운 비율(준비/활성=0)")]
        [SerializeField] Image cooldownFill;
        [Tooltip("선택: 상태 문구(READY/쿨다운초). 없으면 무시")]
        [SerializeField] TMP_Text label;

        PlayerShield _shield;

        void Start()
        {
            _shield = FindAnyObjectByType<PlayerShield>();
            var events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (_shield == null || events == null)
            {
                Debug.LogWarning("[ShieldButtonView] PlayerShield/Events 없음 — 비활성.", this);
                enabled = false;
                return;
            }

            if (button != null) button.onClick.AddListener(() => _shield.TryActivate());
            events.Shield.Subscribe(OnShield).AddTo(this);
        }

        void OnShield(ShieldState s)
        {
            if (button != null) button.interactable = s.Ready;
            if (cooldownFill != null) cooldownFill.fillAmount = s.Cooldown01;
            if (label != null)
            {
                // 쿨다운 중 = 남은 초 카운트(올림). 활성 중 = ON. 준비 = SHIELD.
                label.text = s.Active ? "ON"
                    : s.Ready ? "SHIELD"
                    : Mathf.CeilToInt(s.CooldownRemaining).ToString();
            }
        }
    }
}
