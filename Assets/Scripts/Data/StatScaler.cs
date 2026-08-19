namespace VD.Core
{
    /// <summary>
    /// base 적 스탯 × 전역 배율 → effective (M2-5d). 순수 로직. 빌더가 스폰 시 호출(<see cref="DifficultyProvider"/> 배율 주입).
    /// <para>배율 대상 = <b>체력/속도/데미지</b>(progression-design §2 "전역 스탯 배율"). 처치점수(killScore)와
    /// 공격AI별 필드(fireInterval/projectileSpeed/barrageCount/suicideRadius)는 <b>배율 밖</b>(base 그대로).</para>
    /// </summary>
    public static class StatScaler
    {
        /// <summary><paramref name="baseStats"/>의 체력/속도/데미지에만 <paramref name="multiplier"/>를 곱한 effective 반환.</summary>
        public static EnemyStats Scale(EnemyStats baseStats, float multiplier)
        {
            var s = baseStats;   // 나머지 필드(killScore·공격AI별)는 그대로 복사됨
            s.maxHp = baseStats.maxHp * multiplier;
            s.moveSpeed = baseStats.moveSpeed * multiplier;
            s.damage = baseStats.damage * multiplier;
            return s;
        }
    }
}
