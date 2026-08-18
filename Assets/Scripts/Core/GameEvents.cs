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

        // ── 진행(경험치/레벨) — M1-6 ─────────────────────────────────
        // 오브 습득(입력): Orb가 발행 → ExperienceSystem이 구독·누적.
        // 레벨/경험치%(출력 상태): ExperienceSystem이 갱신 → HUD(M1-10)가 구독.
        // 레벨업(출력 이벤트): ExperienceSystem이 발행 → 3choice(M1-7)가 구독.

        readonly Subject<int> _orbCollected = new();
        readonly ReactiveProperty<int> _level = new(1);
        readonly ReactiveProperty<float> _xpNormalized = new(0f);
        readonly Subject<int> _levelUp = new();

        /// <summary>오브 습득 스트림(습득 경험치량). Orb가 <see cref="PublishOrbCollected"/>로 발행.</summary>
        public Observable<int> OrbCollected => _orbCollected;
        /// <summary>현재 레벨(1부터). 갱신은 ExperienceSystem(internal SetLevel)만.</summary>
        public ReadOnlyReactiveProperty<int> Level => _level;
        /// <summary>현재 레벨 내 경험치 진행도 0~1(HUD 게이지용). 갱신은 ExperienceSystem만.</summary>
        public ReadOnlyReactiveProperty<float> XpNormalized => _xpNormalized;
        /// <summary>레벨업 이벤트(새 레벨). 발행은 ExperienceSystem(internal RaiseLevelUp)만.</summary>
        public Observable<int> LevelUp => _levelUp;

        /// <summary>오브 습득 발행 — VD.Runtime 내부(Orb)에서 호출.</summary>
        internal void PublishOrbCollected(int xp) => _orbCollected.OnNext(xp);
        /// <summary>레벨 갱신 — ExperienceSystem에서만.</summary>
        internal void SetLevel(int level) => _level.Value = level;
        /// <summary>경험치 진행도 갱신 — ExperienceSystem에서만.</summary>
        internal void SetXpNormalized(float normalized) => _xpNormalized.Value = normalized;
        /// <summary>레벨업 발행 — ExperienceSystem에서만.</summary>
        internal void RaiseLevelUp(int newLevel) => _levelUp.OnNext(newLevel);

        public void Dispose()
        {
            _state.Dispose();
            _orbCollected.Dispose();
            _level.Dispose();
            _xpNormalized.Dispose();
            _levelUp.Dispose();
        }
    }
}
