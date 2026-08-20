using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 무기 3종 공통 레벨 머신(M4-2). <see cref="IWeapon"/> 레벨 API를 한곳에 구현해
    /// 각 무기(<see cref="StraightGun"/>/<see cref="HomingMissile"/>/<see cref="Railgun"/>)가 발사 로직만 신경 쓰게 한다.
    /// <para><b>레벨→탄약 매핑</b>(weapon-acquisition §3): 탄약(동시 발사 줄기 수) = 레벨(최대 <see cref="MaxAmmo"/>=4).
    /// Lv1=1발 … Lv4=4발. Lv5는 무기별 특수기능(M5-4)이라 탄약을 더 늘리지 않음 — <see cref="Ammo"/>가 4로 포화.</para>
    /// </summary>
    public abstract class WeaponBase : IWeapon
    {
        /// <summary>탄약 상한(동시 발사 줄기 수). Lv4에서 4발로 포화, Lv5(M5-4)는 특수기능이라 탄약 불변.</summary>
        public const int MaxAmmo = 4;

        public int Level { get; private set; }
        public int MaxLevel { get; }
        public bool IsMaxLevel => Level >= MaxLevel;

        /// <summary>현재 탄약(동시 발사 줄기 수). = min(<see cref="Level"/>, <see cref="MaxAmmo"/>). 파생 무기가 발사 수로 사용.</summary>
        protected int Ammo => Mathf.Min(Level, MaxAmmo);

        // ── 무기별 파워 배율(M4-8, 3choice 무기 파워 카드가 누적) ────────────────
        float _fireRateMult = 1f;    // 연사속도 배율(↑=빠름). 발사 간격에 나눔.
        float _projSpeedMult = 1f;   // 탄속 배율(↑=빠름). 탄속에 곱함.

        /// <summary>연사속도 배율(1=기본). 발사 간격 = 기본간격 / 이 값.</summary>
        protected float FireRateMult => _fireRateMult;
        /// <summary>탄속 배율(1=기본). 탄속 = 기본탄속 × 이 값.</summary>
        protected float ProjectileSpeedMult => _projSpeedMult;

        /// <summary>연사속도 강화(누적, 배수). pct=0.1 → +10%.</summary>
        public void AddFireRate(float pct) => _fireRateMult *= (1f + pct);
        /// <summary>탄속 강화(누적, 배수). pct=0.1 → +10%.</summary>
        public void AddProjectileSpeed(float pct) => _projSpeedMult *= (1f + pct);

        /// <param name="startLevel">시작 레벨(1~maxLevel로 클램프). 인스펙터 검증값 또는 획득(=1).</param>
        /// <param name="maxLevel">최대 레벨. M4-2=4(Lv5 특수는 M5-4에서 5로).</param>
        protected WeaponBase(int startLevel, int maxLevel = 4)
        {
            MaxLevel = Mathf.Max(1, maxLevel);
            Level = Mathf.Clamp(startLevel, 1, MaxLevel);
        }

        public void LevelUp()
        {
            if (Level < MaxLevel) Level++;
        }

        public abstract void Tick(float dt, WeaponContext ctx);
    }
}
