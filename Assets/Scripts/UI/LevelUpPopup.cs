using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VD.Core;
using VD.Player;

namespace VD.UI
{
    /// <summary>
    /// 3choice 강화 선택 팝업 (M1-7, uGUI+TMP). <see cref="GameEvents.LevelUp"/> 구독 →
    /// <see cref="GameManager.Pause"/>(timeScale 0) → <see cref="UpgradeSystem.Roll"/> 3장 카드 표시 →
    /// 클릭 시 <see cref="UpgradeSystem.Apply"/> → 재개. 한 번에 여러 레벨업은 **큐로 순차** 표시.
    /// 카드 텍스트는 <see cref="UpgradeSystem.Describe"/>(실제 수치)에서 받음.
    /// </summary>
    public sealed class LevelUpPopup : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] Button[] cardButtons;
        [SerializeField] TMP_Text[] cardTitles;
        [SerializeField] TMP_Text[] cardDescs;
        [SerializeField] TMP_Text[] cardEffects;

        UpgradeSystem _upgrades;
        List<UpgradeType> _current;
        int _pending;
        bool _showing;

        void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        void Start()
        {
            _upgrades = FindAnyObjectByType<UpgradeSystem>();
            var events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (events == null || _upgrades == null)
            {
                Debug.LogWarning("[LevelUpPopup] GameManager/UpgradeSystem 없음 — 비활성.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < cardButtons.Length; i++)
            {
                int idx = i;   // 클로저 캡처
                cardButtons[i].onClick.AddListener(() => OnCardClicked(idx));
            }

            events.LevelUp.Subscribe(_ => OnLevelUp()).AddTo(this);
        }

        void OnLevelUp()
        {
            _pending++;
            if (!_showing) ShowNext();
        }

        void ShowNext()
        {
            if (_pending <= 0) return;

            _current = _upgrades.Roll(cardButtons.Length);
            for (int i = 0; i < cardButtons.Length; i++)
            {
                bool has = i < _current.Count;
                cardButtons[i].gameObject.SetActive(has);
                if (!has) continue;

                UpgradeDisplay d = _upgrades.Describe(_current[i]);
                if (cardTitles[i] != null) cardTitles[i].text = d.Title;
                if (cardDescs[i] != null) cardDescs[i].text = d.Description;
                if (cardEffects[i] != null) cardEffects[i].text = d.Effect;
            }

            _showing = true;
            if (panelRoot != null) panelRoot.SetActive(true);
            GameManager.Instance?.Pause();   // 이미 Paused면 무시(가드)
        }

        /// <summary>카드 클릭 → 강화 적용 → 다음 대기분 있으면 이어서, 없으면 재개.</summary>
        public void OnCardClicked(int index)
        {
            if (!_showing || _current == null || index < 0 || index >= _current.Count) return;

            _upgrades.Apply(_current[index]);
            _pending--;
            _showing = false;
            if (panelRoot != null) panelRoot.SetActive(false);

            if (_pending > 0) ShowNext();
            else GameManager.Instance?.Resume();
        }
    }
}
