namespace VD.Enemy
{
    /// <summary>
    /// 적 이동 AI 모듈(M3-1). 재사용 전략 — 같은 <see cref="Enemy"/> 셸이 SO(<see cref="VD.Core.MoveAIType"/>)
    /// 설정만으로 다르게 거동한다. 순수 C#(MonoBehaviour 아님) — <see cref="Enemy"/>가 메시지 창구가 되어
    /// <see cref="Tick"/>로 위임한다. 물리 미사용(transform 직접 이동, 적끼리 통과, 사용자 결정 2026-08-20).
    /// <para>상태 없는 모듈(직진/추적)은 빌더가 싱글톤으로 공유 가능. 상태가 있는 모듈(사행 등 M4-7)은
    /// 반드시 <b>인스턴스별</b>로 두고 <see cref="OnSpawned"/>에서 리셋해야 한다.</para>
    /// </summary>
    public interface IMoveBehaviour
    {
        /// <summary>스폰(풀 재사용)마다 호출 — per-instance 상태 리셋 훅. 무상태 모듈은 no-op.</summary>
        void OnSpawned();

        /// <summary>매 프레임 이동. 속도는 <see cref="Enemy.MoveSpeed"/>(데이터), despawn 판정은 <see cref="Enemy"/>가 담당.</summary>
        void Tick(Enemy self, float dt);
    }
}
