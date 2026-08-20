namespace VD.Enemy
{
    /// <summary>
    /// 적 공격 AI 모듈(M3-2). 재사용 전략 — 같은 <see cref="Enemy"/> 셸이 SO(<see cref="VD.Core.AttackAIType"/>)
    /// 설정만으로 다르게 공격한다. 순수 C#(<see cref="IMoveBehaviour"/>와 동형) — <see cref="Enemy"/>가 창구가 되어
    /// Update에서 <see cref="Tick"/>로 위임. 발사가 필요한 모듈은 빌더가 적탄 풀을 주입해 준다.
    /// <para>발사 쿨다운 등 <b>상태가 있는 모듈</b>(탄막)은 인스턴스별로 두고 <see cref="OnSpawned"/>에서 리셋해야 한다.
    /// 무상태 모듈(충돌 no-op/자폭)은 빌더가 싱글톤 공유 가능.</para>
    /// </summary>
    public interface IAttackBehaviour
    {
        /// <summary>스폰(풀 재사용)마다 호출 — 쿨다운 등 per-instance 상태 리셋. 무상태 모듈은 no-op.</summary>
        void OnSpawned();

        /// <summary>매 프레임 공격 판정. 수치(간격/탄속/탄수/자폭반경/데미지)는 <see cref="Enemy"/> stats(데이터).</summary>
        void Tick(Enemy self, float dt);
    }
}
