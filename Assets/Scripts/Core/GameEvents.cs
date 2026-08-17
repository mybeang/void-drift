using System;
using R3;

namespace VD.Core
{
    /// <summary>
    /// 게임 전역 pub/sub 채널. GameManager가 소유하며 상태를 갱신하고,
    /// 다른 시스템(HUD 등)은 여기서 구독만 한다. (상태 갱신 권한은 같은 어셈블리 internal로 제한)
    /// 게임플레이 이벤트(처치/레벨업/오브 등)는 필요해지는 백로그 시점에 여기에 추가한다.
    /// </summary>
    public sealed class GameEvents : IDisposable
    {
        readonly ReactiveProperty<GameState> _state = new(GameState.Boot);

        /// <summary>현재 게임 상태(구독 가능). 변경 권한은 GameManager(internal SetState)만.</summary>
        public ReadOnlyReactiveProperty<GameState> State => _state;

        /// <summary>상태 갱신 — VD.Runtime 내부(GameManager)에서만 호출.</summary>
        internal void SetState(GameState next) => _state.Value = next;

        public void Dispose() => _state.Dispose();
    }
}
