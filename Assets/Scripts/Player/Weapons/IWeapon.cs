namespace VD.Player
{
    /// <summary>
    /// 플레이어 무기 모듈(M4-1). 재사용 전략 — 적 AI(<see cref="VD.Enemy.IMoveBehaviour"/>/<see cref="VD.Enemy.IAttackBehaviour"/>)와
    /// 동형의 순수 C#(MonoBehaviour 아님). 각 무기가 <b>자기 연사 쿨다운</b>을 갖고 <see cref="Tick"/>에서 자기 타이밍에 발사한다.
    /// <see cref="PlayerShooter"/>가 보유 무기 리스트를 매 프레임 틱해 <b>동시 오토발사</b>를 만든다(weapon-acquisition §1).
    /// <para>쿨다운 등 상태를 가지므로 무기는 <b>인스턴스별</b>(싱글톤 공유 금지). 공용 조준/풀/데미지는 <see cref="WeaponContext"/>로 주입.</para>
    /// </summary>
    public interface IWeapon
    {
        /// <summary>매 프레임 발사 판정. dt=Time.deltaTime, ctx=공용 조준·풀·base 데미지(<see cref="WeaponContext"/>).</summary>
        void Tick(float dt, WeaponContext ctx);
    }
}
