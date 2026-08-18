namespace VD.Core
{
    /// <summary>
    /// 적 공격 AI 종류 (enemy-design §3). SO가 값만 보관, 실제 공격 로직은 M3-2.
    /// </summary>
    public enum AttackAIType
    {
        Contact,   // 충돌 — 발사 없음, 몸통 접촉 데미지 (근접 필수)
        AimedShot, // 조준단발 — 플레이어 방향 한 발 (거리 무관)
        Barrage,   // 탄막 — 방사형/부채꼴 다발 (원거리 선호)
        Suicide,   // 자폭 — 근접 시 범위 폭발 (근접 필수)
    }
}
