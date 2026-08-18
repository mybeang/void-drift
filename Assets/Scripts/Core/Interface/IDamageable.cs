namespace VD.Core
{
    /// <summary>
    /// 데미지를 받을 수 있는 대상. 투사체·충돌이 이 인터페이스로 데미지를 전달한다.
    /// 최소 계약(<see cref="TakeDamage"/> 하나) — 히트 위치/넉백/VFX 등 확장은 필요 시 추가. (M1-4 신설)
    /// </summary>
    public interface IDamageable
    {
        /// <summary>데미지를 입는다. 사망 판정·연출은 구현체 책임.</summary>
        void TakeDamage(float amount);
    }
}
