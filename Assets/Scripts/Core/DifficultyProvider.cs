using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 전역 난이도 → 적 스탯 배율 제공 (M4-5, progression-design §2). 적 빌더가 스폰 시 <see cref="StatMultiplier"/>를
    /// base 스탯에 곱해 effective를 만든다(<see cref="StatScaler"/>). base(적 테이블 RO)·배율(여기)·effective(런타임) 3층 분리.
    /// <para><b>곡선</b>: <see cref="phases"/>(순서 배열)를 경과시간(Playing 중)으로 훑어 —
    /// 페이즈 내부는 start→end 선형 <b>미세 상승</b>, 경계는 다음 페이즈 start로 <b>점프</b> + <see cref="GameEvents.DifficultyBanner"/> 안내.
    /// 마지막 페이즈 이후는 그 end 배율로 유지. 수치는 SO(오서링 툴 편집). 리런(재-Playing) 시 경과시간 리셋.</para>
    /// </summary>
    public sealed class DifficultyProvider : MonoBehaviour
    {
        [Tooltip("난이도 페이즈 순서 배열(시간축). Difficulty Authoring 창에서 편집한 SO를 순서대로 배치")]
        [SerializeField] DifficultyPhaseDefinition[] phases = new DifficultyPhaseDefinition[0];

        float _elapsed;
        float _currentMult = 1f;
        int _phaseIndex;
        bool _wasPlaying;

        /// <summary>현재 전역 스탯 배율(체력/속도/데미지). 스폰 시 빌더가 질의.</summary>
        public float StatMultiplier => _currentMult;

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
            if (playing && !_wasPlaying) ResetRun();   // (재)시작 진입 → 경과시간 리셋
            _wasPlaying = playing;
            if (!playing) return;

            _elapsed += Time.deltaTime;   // timeScale 0(일시정지/게임오버)이면 dt=0 → 자연 정지
            Recompute();
        }

        void ResetRun()
        {
            _elapsed = 0f;
            _phaseIndex = 0;
            _currentMult = (phases != null && phases.Length > 0 && phases[0] != null) ? phases[0].startMultiplier : 1f;
        }

        void Recompute()
        {
            if (phases == null || phases.Length == 0) { _currentMult = 1f; return; }

            // 누적 지속시간으로 현재 페이즈 탐색.
            float t = _elapsed;
            int idx = 0;
            while (idx < phases.Length - 1 && phases[idx] != null && t >= phases[idx].durationSeconds)
            {
                t -= phases[idx].durationSeconds;
                idx++;
            }

            var p = phases[idx];
            if (p != null)
            {
                float dur = Mathf.Max(0.01f, p.durationSeconds);
                float frac = Mathf.Clamp01(t / dur);   // 마지막 페이즈는 초과 시 1로 클램프 → end 유지
                _currentMult = Mathf.Lerp(p.startMultiplier, p.endMultiplier, frac);
            }

            // 페이즈 경계 진입 → 안내 문구 발행(첫 페이즈 0은 경계 아님).
            if (idx != _phaseIndex)
            {
                _phaseIndex = idx;
                if (p != null && !string.IsNullOrEmpty(p.bannerText))
                    GameManager.Instance?.Events?.RaiseDifficultyBanner(p.bannerText);
            }
        }
    }
}
