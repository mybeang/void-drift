using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 조립 seam (M2-5e). <see cref="EnemyDefinition"/>(툴 데이터) + 풀 셸(<see cref="Enemy"/>) → 조립된 적.
    /// <list type="number">
    /// <item>① <b>비주얼</b>: 캐시에서 모델 프리팹 Resolve → <see cref="Enemy.AttachVisual"/>로 셸 자식 부착.</item>
    /// <item>② <b>스탯</b>: base(def.stats) × 전역배율(<see cref="DifficultyProvider"/>) = effective → <see cref="Enemy.ApplyStats"/>.</item>
    /// <item>③ <b>AI(M3 자리)</b>: def.moveAI/attackAI로 AI 모듈 부착 — 이번엔 비움(이동은 셸 직진 유지).</item>
    /// </list>
    /// 위치/회전/launch/드랍은 스포너 관심사(여기 아님). "비주얼+스탯만" 스코프(M2-5, 사용자 결정).
    /// </summary>
    public sealed class EnemyBuilder
    {
        readonly EnemyVisualCache _cache;
        readonly DifficultyProvider _difficulty;

        public EnemyBuilder(EnemyVisualCache cache, DifficultyProvider difficulty)
        {
            _cache = cache;
            _difficulty = difficulty;
        }

        /// <summary>풀에서 꺼낸 셸에 def를 조립(비주얼 부착 + effective 스탯 주입). null 안전.</summary>
        public void Build(Enemy shell, EnemyDefinition def)
        {
            if (shell == null || def == null) return;

            // ① 비주얼 (미리 로드된 프리팹, 없으면 null → 부착 안 함)
            shell.AttachVisual(_cache != null ? _cache.Resolve(def.visual) : null);

            // ② 스탯: base × 전역배율 → effective (배율 소스 없으면 1.0)
            float mult = _difficulty != null ? _difficulty.StatMultiplier : 1f;
            shell.ApplyStats(StatScaler.Scale(def.stats, mult));

            // ③ [M3] AI 모듈 부착 자리 (def.moveAI / def.attackAI)
        }
    }
}
