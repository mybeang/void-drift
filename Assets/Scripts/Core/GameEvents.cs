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

        // ── 종료/점수 — M1-9 ─────────────────────────────────────────
        // 처치(입력): Enemy가 실사망 시 발행 → ScoreSystem이 구독·합산.
        // HP%/점수/생존시간(출력 상태): PlayerHealth·ScoreSystem이 갱신 → HUD(M1-10)·결과 구독.

        readonly Subject<int> _enemyKilled = new();
        readonly ReactiveProperty<float> _hpNormalized = new(1f);
        readonly ReactiveProperty<HpAmount> _hpValues = new(new HpAmount(1f, 1f));
        readonly ReactiveProperty<int> _score = new(0);
        readonly ReactiveProperty<float> _survivalTime = new(0f);

        /// <summary>적 처치 스트림(처치 점수). Enemy가 <see cref="PublishEnemyKilled"/>로 발행.</summary>
        public Observable<int> EnemyKilled => _enemyKilled;
        /// <summary>플레이어 HP 0~1(HUD 게이지용). 갱신은 PlayerHealth(internal SetHpNormalized)만.</summary>
        public ReadOnlyReactiveProperty<float> HpNormalized => _hpNormalized;
        /// <summary>플레이어 HP 절대값(현재/최대, HUD 숫자 표기용). 갱신은 PlayerHealth(internal SetHpValues)만.</summary>
        public ReadOnlyReactiveProperty<HpAmount> HpValues => _hpValues;
        /// <summary>현재 점수(생존시간+처치). 갱신은 ScoreSystem만.</summary>
        public ReadOnlyReactiveProperty<int> Score => _score;
        /// <summary>생존시간(초). 갱신은 ScoreSystem만.</summary>
        public ReadOnlyReactiveProperty<float> SurvivalTime => _survivalTime;

        /// <summary>적 처치 발행 — VD.Runtime 내부(Enemy)에서 호출.</summary>
        internal void PublishEnemyKilled(int score) => _enemyKilled.OnNext(score);
        /// <summary>HP 진행도 갱신 — PlayerHealth에서만.</summary>
        internal void SetHpNormalized(float normalized) => _hpNormalized.Value = normalized;
        /// <summary>HP 절대값 갱신 — PlayerHealth에서만.</summary>
        internal void SetHpValues(float current, float max) => _hpValues.Value = new HpAmount(current, max);
        /// <summary>점수 갱신 — ScoreSystem에서만.</summary>
        internal void SetScore(int score) => _score.Value = score;
        /// <summary>생존시간 갱신 — ScoreSystem에서만.</summary>
        internal void SetSurvivalTime(float seconds) => _survivalTime.Value = seconds;

        public void Dispose()
        {
            _state.Dispose();
            _orbCollected.Dispose();
            _level.Dispose();
            _xpNormalized.Dispose();
            _levelUp.Dispose();
            _enemyKilled.Dispose();
            _hpNormalized.Dispose();
            _hpValues.Dispose();
            _score.Dispose();
            _survivalTime.Dispose();
        }
    }

    /// <summary>HUD 숫자 표기용 HP 절대값(현재/최대). 값 동등성으로 불변 시 재발행 안 됨.</summary>
    public readonly struct HpAmount
    {
        public readonly float Current;
        public readonly float Max;
        public HpAmount(float current, float max) { Current = current; Max = max; }
    }
}
