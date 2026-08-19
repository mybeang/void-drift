using System.Collections.Generic;

namespace VD.Core
{
    /// <summary>유효성 경고 하나 — 메시지 + 관련 필드(하이라이트용, <see cref="EnemyDefinition"/> 직렬화 필드명).</summary>
    public readonly struct EnemyWarning
    {
        public readonly string Message;
        public readonly string[] Fields;   // 예: "moveAI","attackAI","archetype","visual"

        public EnemyWarning(string message, params string[] fields)
        {
            Message = message;
            Fields = fields;
        }
    }

    /// <summary>
    /// 적 조합 유효성 판정 (M2-4, enemy-design §4). <b>차단 아님 — 경고만.</b> 순수 로직(런타임도 접근 가능).
    /// <list type="bullet">
    /// <item><b>R1</b> AttackAI 요구거리 ↔ MoveAI 거리성향 모순(근접필수+거리유지).</item>
    /// <item><b>R2</b> archetype 교전거리(<see cref="EnemyDefinition.RangeLabelOf"/>) ↔ AttackAI 요구거리 모순.</item>
    /// </list>
    /// R3(비주얼 Addressables `archetype:` 라벨 교차, §6)는 <b>에디터 전용 API</b>라 VD.Editor에서 별도 수행.
    /// 메타(거리성향/요구거리)는 M2-2 결정 #5에서 여기(M2-4)로 미뤄둔 유효성 메타.
    /// </summary>
    public static class EnemyValidation
    {
        public enum DistanceTendency { Approach, KeepDistance }
        public enum RequiredRange { Melee, Any, Ranged }

        /// <summary>MoveAI 거리 성향 (§3): Hover=거리유지, 그 외(직진/추적/사행)=접근.</summary>
        public static DistanceTendency TendencyOf(MoveAIType move) =>
            move == MoveAIType.Hover ? DistanceTendency.KeepDistance : DistanceTendency.Approach;

        /// <summary>AttackAI 요구 교전거리 (§3): 충돌/자폭=근접필수, 탄막=원거리선호, 조준단발=무관.</summary>
        public static RequiredRange RangeOf(AttackAIType attack)
        {
            switch (attack)
            {
                case AttackAIType.Contact:
                case AttackAIType.Suicide: return RequiredRange.Melee;
                case AttackAIType.Barrage: return RequiredRange.Ranged;
                default: return RequiredRange.Any;   // AimedShot
            }
        }

        /// <summary>R1·R2 판정 → 경고 목록(없으면 빈 목록). R3(라벨 교차)은 에디터에서 이 결과에 덧붙인다.</summary>
        public static List<EnemyWarning> Validate(EnemyDefinition def)
        {
            var list = new List<EnemyWarning>();
            if (def == null) return list;

            var req = RangeOf(def.attackAI);

            // R1: 근접필수 공격인데 이동이 거리유지 → 붙지 않아 발동 안 함
            if (req == RequiredRange.Melee && TendencyOf(def.moveAI) == DistanceTendency.KeepDistance)
                list.Add(new EnemyWarning(
                    $"근접 공격({def.attackAI})인데 이동이 거리유지({def.moveAI}) — 붙지 않아 발동하지 못함.",
                    "attackAI", "moveAI"));

            // R2: archetype 교전거리 ↔ AttackAI 요구거리 모순
            string range = EnemyDefinition.RangeLabelOf(def.archetype);
            if (range == "근거리" && req == RequiredRange.Ranged)
                list.Add(new EnemyWarning(
                    $"근거리형({def.archetype})인데 원거리 공격({def.attackAI}).",
                    "archetype", "attackAI"));
            else if (range == "원거리" && req == RequiredRange.Melee)
                list.Add(new EnemyWarning(
                    $"원거리형({def.archetype})인데 근접 공격({def.attackAI}).",
                    "archetype", "attackAI"));

            return list;
        }
    }
}
