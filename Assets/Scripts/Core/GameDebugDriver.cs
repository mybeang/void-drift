using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace VD.Core
{
    /// <summary>
    /// [디버그 · 에디터 전용] 키보드로 상태 전이를 트리거한다.
    /// 원래 M1-1 수동 검증용 임시 코드였으나(M1-2 입력·M1-9 게임오버로 대체됨),
    /// ResultScene(M2) 전까지 게임오버 후 재시작(R) 등 테스트 편의를 위해 유지한다.
    /// P: Pause/Resume 토글 · G: GameOver · R: (GameOver 후) StartGame 재진입.
    /// New Input System(Keyboard)만 사용 — 레거시 Input.* 미사용.
    /// Update는 timeScale=0에서도 호출되므로 일시정지 중에도 P로 재개 가능.
    /// 동작은 <c>#if UNITY_EDITOR</c> 가드로 에디터에서만 — 빌드에선 무동작(inert).
    /// </summary>
    public sealed class GameDebugDriver : MonoBehaviour
    {
#if UNITY_EDITOR
        void Update()
        {
            var kb = Keyboard.current;
            var gm = GameManager.Instance;
            if (kb == null || gm == null) return;

            if (kb.pKey.wasPressedThisFrame)
            {
                if (gm.State == GameState.Playing) gm.Pause();
                else if (gm.State == GameState.Paused) gm.Resume();
            }

            if (kb.gKey.wasPressedThisFrame) gm.GameOver();

            if (kb.rKey.wasPressedThisFrame && gm.State == GameState.GameOver) gm.StartGame();
        }
#endif
    }
}
