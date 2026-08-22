namespace VD.Core
{
    /// <summary>
    /// 플레이어가 입은 데미지의 출처 — 피격 SFX 분기용(M5-5).
    /// <see cref="VD.Player.PlayerHealth.ApplyDamage(float, DamageSource)"/>가 이 값으로 사운드를 고른다.
    /// </summary>
    public enum DamageSource
    {
        /// <summary>적 접촉(돌진). → sfx_chargeAttack</summary>
        Contact,
        /// <summary>적 탄환 피격. → sfx_hitPlayer</summary>
        Bullet,
        /// <summary>자폭 폭발 피해. → sfx_hitPlayer</summary>
        Suicide,
    }
}
