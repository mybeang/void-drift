using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 조립 seam (M2-5e). <see cref="EnemyDefinition"/>(툴 데이터) + 풀 셸(<see cref="Enemy"/>) → 조립된 적.
    /// <list type="number">
    /// <item>① <b>비주얼</b>: 캐시에서 모델 프리팹 Resolve → <see cref="Enemy.AttachVisual"/>로 셸 자식 부착.</item>
    /// <item>② <b>스탯</b>: base(def.stats) × 전역배율(<see cref="DifficultyProvider"/>) = effective → <see cref="Enemy.ApplyStats"/>.</item>
    /// <item>③ <b>AI</b>: def.moveAI로 이동 모듈(<see cref="IMoveBehaviour"/>, M3-1) + def.attackAI로 공격 모듈(<see cref="IAttackBehaviour"/>, M3-2) 부착.</item>
    /// </list>
    /// 위치/회전/launch/드랍은 스포너 관심사(여기 아님). "비주얼+스탯만" 스코프(M2-5, 사용자 결정).
    /// </summary>
    public sealed class EnemyBuilder
    {
        readonly EnemyVisualCache _cache;
        readonly DifficultyProvider _difficulty;
        readonly EnemyBulletPool _bulletPool;   // 탄막 발사용(M3-2). 없으면 탄막 무발사.

        // 이동 AI 모듈(M3-1). 무상태라 싱글톤 공유(스폰마다 재할당 없음).
        // ⚠ 상태 있는 모듈(사행 등 M4-7 추가 시)은 여기 공유하지 말고 인스턴스별로 생성할 것.
        readonly IMoveBehaviour _straight = new StraightMove();
        readonly IMoveBehaviour _chase = new ChaseMove();

        // 공격 AI 모듈(M3-2). 무상태(충돌)만 싱글톤 공유. 탄막/조준단발/자폭(I-6 상태화)은 스폰마다 new(ResolveAttack).
        readonly IAttackBehaviour _contact = new ContactAttack();

        public EnemyBuilder(EnemyVisualCache cache, DifficultyProvider difficulty, EnemyBulletPool bulletPool)
        {
            _cache = cache;
            _difficulty = difficulty;
            _bulletPool = bulletPool;
        }

        /// <summary>풀에서 꺼낸 셸에 def를 조립(비주얼 부착 + effective 스탯 주입). null 안전.</summary>
        public void Build(Enemy shell, EnemyDefinition def)
        {
            if (shell == null || def == null) return;

            // ① 비주얼 (미리 로드된 프리팹, 없으면 null → 부착 안 함) + 크기 배수(M3-3, 0이하는 1)
            float vScale = def.visualScale > 0f ? def.visualScale : 1f;
            shell.AttachVisual(_cache != null ? _cache.Resolve(def.visual) : null, vScale);

            // ② 스탯: 현재 base 그대로. 웨이브 곡선(healthClass/공격AI → WaveDifficultyConfig) 배선은 3-2.
            //    (구 단일 전역배율 폐기 — DifficultyProvider는 웨이브·스탯곡선 소스로 개편됨.)
            shell.ApplyStats(def.stats);

            // ③ AI 모듈 부착: 이동(M3-1) + 공격(M3-2).
            shell.SetMoveBehaviour(ResolveMove(def.moveAI));
            shell.SetAttackBehaviour(ResolveAttack(def.attackAI));

            // ④ 아키타입 주입(M4-1) — 유도 미사일 조준 타입 우선순위 분류용.
            shell.SetArchetype(def.archetype);
        }

        /// <summary>def.moveAI → 이동 모듈. Weave/Hover는 per-instance 상태(위상·페이즈)라 스폰마다 new. 직진/추적은 싱글톤.</summary>
        IMoveBehaviour ResolveMove(MoveAIType type) => type switch
        {
            MoveAIType.Chase => _chase,
            MoveAIType.Weave => new WeaveMove(),
            MoveAIType.Hover => new HoverMove(),
            _ => _straight,   // Straight 폴백
        };

        /// <summary>def.attackAI → 공격 모듈. 탄막/조준단발은 쿨다운 상태라 인스턴스별 new. 그 외는 싱글톤.</summary>
        IAttackBehaviour ResolveAttack(AttackAIType type) => type switch
        {
            AttackAIType.Barrage => new BarrageAttack(_bulletPool),
            AttackAIType.AimedShot => new AimedShot(_bulletPool),
            AttackAIType.Suicide => new SuicideAttack(),   // I-6: Arm 상태(멈춤+깜박+타이머)라 인스턴스별
            _ => _contact,   // Contact no-op 폴백
        };
    }
}
