using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// GameScene 수명의 게임 상태 관리자(싱글톤). 상태 전이 + 시간 스케일 제어를 담당하고,
    /// 전역 이벤트 채널 <see cref="GameEvents"/> 를 소유·노출한다.
    /// 씬 한정 수명(DontDestroyOnLoad 아님): GameScene 로드마다 새로 생성된다.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        GameEvents _events;

        /// <summary>전역 이벤트 채널(구독용).</summary>
        public GameEvents Events => _events;

        /// <summary>현재 상태(스냅샷). 스트림 구독은 <see cref="GameEvents.State"/>.</summary>
        public GameState State => _events.State.CurrentValue;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameManager] 인스턴스가 이미 존재 — 중복 제거.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _events = new GameEvents();
        }

        void Start()
        {
            // 실제 리소스 로딩/FadeOut은 M2(Addressables) 이후. 지금은 즉시 Playing 진입.
            StartGame();
        }

        /// <summary>Boot → Playing. 시간 정상화.</summary>
        public void StartGame()
        {
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }

        /// <summary>Playing → Paused. 시간 정지(3choice 팝업 등). Playing이 아니면 무시.</summary>
        public void Pause()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }

        /// <summary>Paused → Playing. 시간 재개. Paused가 아니면 무시.</summary>
        public void Resume()
        {
            if (State != GameState.Paused) return;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }

        /// <summary>→ GameOver. GameScene 정지형(M1-9): timeScale 0으로 스폰·사격·이동 정지. 결과값은 GameEvents에 보관.
        /// ResultScene 전환·결과 UI는 이후(M1-10/M2). 중복 전이 무시.</summary>
        public void GameOver()
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.GameOver);
            Time.timeScale = 0f;   // 정지형 — 화면 프리즈
        }

        void SetState(GameState next)
        {
            var prev = State;
            if (prev == next) return;
            _events.SetState(next);
            Debug.Log($"[GameManager] 상태 전이: {prev} → {next}");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            _events?.Dispose();
            // 씬을 떠날 때 timeScale이 0인 채로 남지 않게 복구.
            Time.timeScale = 1f;
        }
    }
}
