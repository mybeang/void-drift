using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 웨이브 난이도 런타임 소스(밸런싱 패스 3-1·3-2). 데이터는 전부 <see cref="WaveDifficultyConfig"/>(오서링 툴 편집),
    /// 여기선 경과시간→웨이브를 세고 config의 공식으로 <b>동시상한·소환주기·스탯·승급배너</b>를 제공한다.
    /// <para>(구 페이즈 배율 스텁 폐기 — 스탯은 단일배율이 아니라 스탯별 곡선(config).) 리런(재-Playing) 시 리셋.</para>
    /// </summary>
    public sealed class DifficultyProvider : MonoBehaviour
    {
        [Tooltip("웨이브 난이도 설정 SO(Difficulty Authoring 편집). 비우면 안전 폴백(wave만).")]
        [SerializeField] WaveDifficultyConfig config;

        float _elapsed;
        GameState _prevState = GameState.Boot;
        int _lastWave = 1;

        /// <summary>편집 데이터(툴/디버그).</summary>
        public WaveDifficultyConfig Config => config;

        /// <summary>현재 웨이브. 1웨이브=config.secondsPerWave(기본 60초).</summary>
        public int CurrentWave => config != null ? config.WaveAt(_elapsed) : Mathf.FloorToInt(_elapsed / 60f) + 1;

        /// <summary>현재 동시 필드 적 상한.</summary>
        public int CurrentCap => config != null ? config.CapAt(CurrentWave) : 10;

        /// <summary>현재 소환 주기(초).</summary>
        public float CurrentInterval => config != null ? config.IntervalAt(CurrentWave) : 2f;

        /// <summary>현재 웨이브의 스탯 값. 아직 미등장(예: 고체력 wave&lt;40)이면 false.</summary>
        public bool TryStatValue(WaveStatKind kind, out float value)
        {
            value = 0f;
            return config != null && config.TryStatValue(kind, CurrentWave, _elapsed, out value);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            GameState state = gm == null ? GameState.Playing : gm.State;

            // (재)시작 진입 시에만 리셋 — 일시정지(Paused)→재개는 이어감(설정창 열어도 웨이브 유지).
            if (state == GameState.Playing && _prevState != GameState.Playing && _prevState != GameState.Paused)
                ResetRun();
            _prevState = state;
            if (state != GameState.Playing) return;
            if (gm != null && gm.CombatFrozen) return;   // 튜토리얼 등 전투 정지 중엔 웨이브 시계도 멈춤

            _elapsed += Time.deltaTime;   // timeScale 0(일시정지/게임오버)이면 dt=0 → 자연 정지

            int w = CurrentWave;
            if (w != _lastWave)
            {
                OnWaveChanged(w);
                _lastWave = w;
            }
        }

        void ResetRun()
        {
            _elapsed = 0f;
            _lastWave = 1;   // wave 1 시작(승급 배너 없음)
        }

        void OnWaveChanged(int wave)
        {
            if (config == null) return;
            string msg = config.BannerFor(wave);   // ≤60 로테이션 / 61+ 고정 / wave1 없음
            Debug.Log($"[TEMP] Wave {wave} 진입 — banner: {(string.IsNullOrEmpty(msg) ? "(없음)" : msg)}");   // 검증용
            if (!string.IsNullOrEmpty(msg))
                GameManager.Instance?.Events?.RaiseDifficultyBanner(msg);
        }
    }
}
