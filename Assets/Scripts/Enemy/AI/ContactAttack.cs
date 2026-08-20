namespace VD.Enemy
{
    /// <summary>
    /// 충돌 공격(M3-2). 별도 발사 없음 — 이동 AI가 플레이어에 접근시키고, 몸통 접촉 데미지는
    /// <see cref="VD.Player.PlayerHealth"/>가 트리거로 감지(<see cref="Enemy.ContactDamage"/> 사용). 따라서 no-op.
    /// 미구현 공격AI(조준단발, M4-7)도 당장은 여기로 폴백(발사 없음). 무상태 → 싱글톤 공유.
    /// </summary>
    public sealed class ContactAttack : IAttackBehaviour
    {
        public void OnSpawned() { }
        public void Tick(Enemy self, float dt) { }
    }
}
