namespace VD.Core
{
    /// <summary>
    /// 3choice 공용 스탯 강화 종류 (M1-8 최소, 하드코딩).
    /// 공격력/연사 등 <b>무기 스코프</b> 강화는 무기 개발(M4) 후 별도. 여기선 무기와 무관한 공용 스탯만.
    /// 효과 적용 방식은 능력치별 상이(이동=배율%, 최대체력=가산, 자석범위=가산) — 적용은 UpgradeSystem.
    /// </summary>
    public enum UpgradeType
    {
        MoveSpeed,      // 이동속도(드래그 게인) 배율 ↑
        MaxHp,          // 최대 체력 가산 ↑ (늘린 만큼 회복)
        MagnetRadius,   // 오브 자석 반경 가산 ↑
    }
}
