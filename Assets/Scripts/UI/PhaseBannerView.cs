using R3;
using TMPro;
using UnityEngine;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 난이도 페이즈 경계 안내 배너 (M4-5, uGUI+TMP). <see cref="GameEvents.DifficultyBanner"/> 구독 →
    /// 문구를 페이드 인/유지/페이드 아웃으로 잠깐 표시("공허 속 적이 더욱 강해졌습니다" 등). 표시 전용(입력 없음).
    /// timeScale 무관하게 뜨도록 unscaled 시간 사용.
    /// </summary>
    public sealed class PhaseBannerView : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;   // 페이드용(alpha)
        [SerializeField] TMP_Text text;
        [Tooltip("페이드 인(초)")]
        [SerializeField] float fadeIn = 0.4f;
        [Tooltip("완전 표시 유지(초)")]
        [SerializeField] float hold = 2f;
        [Tooltip("페이드 아웃(초)")]
        [SerializeField] float fadeOut = 0.8f;

        float _shownAt = -999f;
        bool _active;

        // GameObject는 항상 활성 유지(구독이 이 컴포넌트에 있음). 표시는 alpha로만 제어.
        void Awake()
        {
            if (group != null) group.alpha = 0f;
        }

        void Start()
        {
            var events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (events == null) { enabled = false; return; }
            events.DifficultyBanner.Subscribe(OnBanner).AddTo(this);
        }

        void OnBanner(string message)
        {
            if (text != null) text.text = message;
            _shownAt = Time.unscaledTime;
            _active = true;
        }

        void Update()
        {
            if (!_active || group == null) return;
            float e = Time.unscaledTime - _shownAt;
            float total = fadeIn + hold + fadeOut;

            if (e >= total)
            {
                group.alpha = 0f;
                _active = false;
                return;
            }

            group.alpha =
                e < fadeIn ? e / Mathf.Max(0.0001f, fadeIn) :
                e < fadeIn + hold ? 1f :
                1f - (e - fadeIn - hold) / Mathf.Max(0.0001f, fadeOut);
        }
    }
}
