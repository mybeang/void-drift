namespace VD.Player
{
    /// <summary>
    /// 무기 식별자(M4-3). <see cref="PlayerShooter"/>의 무기 슬롯 조회·획득/레벨업 키 +
    /// 3choice 무기 카드(<see cref="VD.Core.UpgradeType"/> Weapon*)의 라우팅 대상.
    /// </summary>
    public enum WeaponId
    {
        Straight,   // 기관총(시작 로드아웃)
        Homing,     // 유도 미사일
        Railgun,    // 레일건
    }
}
