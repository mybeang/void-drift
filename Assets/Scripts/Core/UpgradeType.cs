namespace VD.Core
{
    /// <summary>
    /// 3choice 공용 스탯 강화 종류 (M3-4에서 데이터화 — 수치/가중치/표시는 <see cref="UpgradeDefinition"/> SO).
    /// enum은 <b>Apply 라우팅 키</b>로만 남는다(타입별 적용 대상·방식이 코드로 갈림). 무기별 파워(연사/탄속/관통)는 M4-8.
    /// </summary>
    public enum UpgradeType
    {
        MoveSpeed,      // 이동속도(드래그 게인) 배율 ↑ (PlayerMovement)
        MaxHp,          // 최대 체력 가산 ↑ (늘린 만큼 회복, PlayerHealth)
        MagnetRadius,   // 오브 자석 반경 가산 ↑ (OrbPool)
        HpRegen,        // 초당 체력 재생 ↑ (PlayerHealth) — M3-4 신규, I-3 대응
        OrbValue,       // 오브 획득 경험치 배수 ↑ (ExperienceSystem) — M3-4 신규
        AttackPower,    // 기초 공격력(투사체 데미지) 가산 ↑ (PlayerShooter) — M3-4 신규. 무기별 배율은 M4-8
    }
}
