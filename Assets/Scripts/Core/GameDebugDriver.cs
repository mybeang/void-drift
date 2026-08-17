using UnityEngine;
using UnityEngine.InputSystem;

namespace VD.Core
{
    /// <summary>
    /// [임시 · M1-1 수동 검증용] 키보드로 상태 전이를 트리거한다.
    /// 실제 입력 스킴은 M1-2, 게임오버 트리거는 M1-9에서 대체되며 이 파일은 그때 삭제한다.
    /// P: Pause/Resume 토글 · G: GameOver · R: (GameOver 후) StartGame 재진입.
    /// New Input System(Keyboard)만 사용 — 레거시 Input.* 미사용.
    /// Update는 timeScale=0에서도 호출되므로 일시정지 중에도 P로 재개 가능.
    /// </summary>
    public sealed class GameDebugDriver : MonoBehaviour
    {
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
    }
}
