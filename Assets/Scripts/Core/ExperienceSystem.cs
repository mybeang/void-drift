using R3;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 경험치 누적·레벨업 (M1-6). <see cref="GameEvents.OrbCollected"/> 구독 → 누적,
    /// 점증 임계값(<b>지수형</b> <c>base×growth^(n-1)</c>) 도달 시 레벨업.
    /// 상태(레벨/경험치%)는 <see cref="GameEvents"/>에 갱신(HUD M1-10 구독), 레벨업은 <see cref="GameEvents.LevelUp"/> 발행(3choice M1-7 구독).
    /// 임계값 수치(base/growth)는 Day5 튜닝(→ 곡선 데이터/SO는 이후). GameScene에 1개 배치.
    /// </summary>
    public sealed class ExperienceSystem : MonoBehaviour
    {
        [Tooltip("레벨1→2 필요 경험치(오브 수). 점증 곡선의 base. 수치 Day5")]
        [SerializeField] float baseThreshold = 5f;
        [Tooltip("레벨당 임계값 배수(지수형 growth). threshold(n)=base×growth^(n-1). 수치 Day5")]
        [SerializeField] float growth = 1.3f;

        GameEvents _events;
        int _level = 1;
        float _xpIntoLevel;
        float _threshold;

        void Start()
        {
            _events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (_events == null)
            {
                Debug.LogWarning("[ExperienceSystem] GameManager/Events 없음 — 비활성.", this);
                enabled = false;
                return;
            }

            _threshold = ThresholdFor(_level);
            _events.SetLevel(_level);
            _events.SetXpNormalized(0f);
            _events.OrbCollected.Subscribe(OnOrbCollected).AddTo(this);
        }

        void OnOrbCollected(int xp)
        {
            _xpIntoLevel += xp;

            while (_xpIntoLevel >= _threshold)   // 초과분 이월, 한 번에 여러 레벨 가능
            {
                _xpIntoLevel -= _threshold;
                _level++;
                _threshold = ThresholdFor(_level);
                _events.SetLevel(_level);
                _events.RaiseLevelUp(_level);
                Debug.Log($"[TEMP] 레벨업 → Lv {_level} (다음 임계 {_threshold:0.#})", this);   // 3choice(M1-7) 전까지 임시
            }

            _events.SetXpNormalized(Mathf.Clamp01(_xpIntoLevel / _threshold));
        }

        float ThresholdFor(int level) => baseThreshold * Mathf.Pow(growth, level - 1);
    }
}
