namespace VD.Player
{
    /// <summary>
    /// 플레이어 무기 모듈(M4-1). 재사용 전략 — 적 AI(<see cref="VD.Enemy.IMoveBehaviour"/>/<see cref="VD.Enemy.IAttackBehaviour"/>)와
    /// 동형의 순수 C#(MonoBehaviour 아님). 각 무기가 <b>자기 연사 쿨다운</b>을 갖고 <see cref="Tick"/>에서 자기 타이밍에 발사한다.
    /// <see cref="PlayerShooter"/>가 보유 무기 리스트를 매 프레임 틱해 <b>동시 오토발사</b>를 만든다(weapon-acquisition §1).
    /// <para>쿨다운 등 상태를 가지므로 무기는 <b>인스턴스별</b>(싱글톤 공유 금지). 공용 조준/풀/데미지는 <see cref="WeaponContext"/>로 주입.</para>
    /// <para><b>레벨(M4-2)</b>: Lv1~4 = 탄약(동시 발사 줄기 수) 1~4(weapon-acquisition §3). Lv5 특수기능=M5-4(범위 밖).
    /// 레벨 상태·클램프는 <see cref="WeaponBase"/>가 공유. <see cref="PlayerShooter"/>가 시작 레벨 주입, M4-3 무기카드가 <see cref="LevelUp"/> 구동.</para>
    /// </summary>
    public interface IWeapon
    {
        /// <summary>매 프레임 발사 판정. dt=Time.deltaTime, ctx=공용 조준·풀·base 데미지(<see cref="WeaponContext"/>).</summary>
        void Tick(float dt, WeaponContext ctx);

        /// <summary>현재 무기 레벨(1~<see cref="MaxLevel"/>). Lv1=획득. 탄약=레벨(최대 4).</summary>
        int Level { get; }

        /// <summary>이 무기의 최대 레벨(M4-2=4, Lv5 특수는 M5-4에서 5로 상향).</summary>
        int MaxLevel { get; }

        /// <summary><see cref="Level"/> >= <see cref="MaxLevel"/> — 더 올릴 수 없음(3choice 풀에서 제외 판정, M4-3).</summary>
        bool IsMaxLevel { get; }

        /// <summary>한 단계 레벨업(<see cref="MaxLevel"/>에서 포화). M4-3 무기카드 재픽업이 호출.</summary>
        void LevelUp();

        /// <summary>연사속도 강화(누적, 배수). M4-8 무기 파워 카드가 호출.</summary>
        void AddFireRate(float pct);
        /// <summary>탄속 강화(누적, 배수). M4-8 무기 파워 카드가 호출.</summary>
        void AddProjectileSpeed(float pct);
    }
}
