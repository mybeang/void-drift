namespace VD.Core
{
    /// <summary>
    /// 적 이동 AI 종류 (enemy-design §3). SO가 값만 보관, 실제 이동 로직은 M3-1.
    /// </summary>
    public enum MoveAIType
    {
        Straight, // 직진 — 코스 따라 곧장 접근
        Chase,    // 추적 — 플레이어 XY 향해 보정하며 접근
        Weave,    // 사행 — 좌우 사인파로 흔들며 접근
        Hover,    // 견제(호버) — 일정 거리에서 코스 속도에 맞춰 머묾
    }
}
