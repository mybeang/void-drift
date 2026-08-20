using System.Collections.Generic;
using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 3choice 강화 롤·적용·표시 (M3-4: 데이터화). 강화 정의는 <see cref="UpgradeDefinition"/> SO 풀(<see cref="pool"/>)에서 온다 —
    /// 수치/문구/가중치/스택상한은 SO(오서링 창에서 편집), <b>적용 방식(라우팅)만</b> 여기 코드. 팝업(M1-7 <c>LevelUpPopup</c>)이 구동.
    /// <para><see cref="Roll"/>=가중치 기반·중복없음(+maxStacks 도달분 제외), <see cref="Apply"/>=type 라우팅, <see cref="Describe"/>=SO에서 렌더.
    /// 스택형 누적(반복 획득 시 효과 누적, upgrade-pool §1). 수치는 Day5(SO). 무기별 파워는 M4-8.</para>
    /// </summary>
    public sealed class UpgradeSystem : MonoBehaviour
    {
        [Header("강화 풀 (SO 데이터)")]
        [Tooltip("3choice 후보 = UpgradeDefinition SO 배열. 오서링 창(Window/Void Drift/Upgrade Authoring)에서 편집한 SO를 배치")]
        [SerializeField] UpgradeDefinition[] pool = new UpgradeDefinition[0];

        [Header("적용 대상(비우면 씬 자동 탐색)")]
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerHealth health;
        [SerializeField] OrbPool orbPool;
        [SerializeField] PlayerShooter shooter;
        [SerializeField] ExperienceSystem experience;
        [SerializeField] PlayerShield shield;

        /// <summary>무기 카드 등장 주기(플레이어 레벨). 5의 배수마다 무기 카드 최소 1개 보장(weapon-acquisition §4).</summary>
        const int MilestoneInterval = 5;

        readonly Dictionary<UpgradeType, int> _stacks = new Dictionary<UpgradeType, int>();
        readonly List<UpgradeDefinition> _candidates = new List<UpgradeDefinition>();
        readonly List<UpgradeDefinition> _weaponCandidates = new List<UpgradeDefinition>();
        readonly List<UpgradeDefinition> _result = new List<UpgradeDefinition>();

        void Awake()
        {
            if (movement == null) movement = FindAnyObjectByType<PlayerMovement>();
            if (health == null) health = FindAnyObjectByType<PlayerHealth>();
            if (orbPool == null) orbPool = FindAnyObjectByType<OrbPool>();
            if (shooter == null) shooter = FindAnyObjectByType<PlayerShooter>();
            if (experience == null) experience = FindAnyObjectByType<ExperienceSystem>();
            if (shield == null) shield = FindAnyObjectByType<PlayerShield>();
        }

        int StackOf(UpgradeType t) => _stacks.TryGetValue(t, out int n) ? n : 0;

        /// <summary>
        /// 가중치 기반 중복없는 롤(최대 count개). weight 0·maxStacks 도달·부적격 종은 제외.
        /// <paramref name="playerLevel"/>이 5의 배수(마일스톤)면 <b>무기 카드 최소 1개 보장</b>(있을 때),
        /// 나머지는 일반 추첨(무기 카드 추가 등장 가능). 마일스톤 아니면 무기 카드는 후보에서 배제.
        /// </summary>
        public List<UpgradeDefinition> Roll(int count, int playerLevel)
        {
            bool milestone = playerLevel > 0 && playerLevel % MilestoneInterval == 0;

            _candidates.Clear();
            foreach (var d in pool)
                if (IsEligible(d, milestone)) _candidates.Add(d);

            _result.Clear();

            // 마일스톤: 적격 무기 카드 중 1장을 먼저 가중 추첨해 확정(최소 1개 보장).
            if (milestone)
            {
                _weaponCandidates.Clear();
                foreach (var d in _candidates)
                    if (TryWeaponId(d.type, out _)) _weaponCandidates.Add(d);
                if (_weaponCandidates.Count > 0)
                {
                    UpgradeDefinition weapon = _weaponCandidates[PickWeighted(_weaponCandidates)];
                    _result.Add(weapon);
                    _candidates.Remove(weapon);
                }
            }

            // 나머지 슬롯 = 남은 후보(스탯 + 마일스톤이면 무기 카드도 포함)에서 중복없이 가중 추첨.
            int remaining = Mathf.Min(count, _result.Count + _candidates.Count) - _result.Count;
            for (int k = 0; k < remaining; k++)
            {
                int pick = PickWeighted(_candidates);
                _result.Add(_candidates[pick]);
                _candidates.RemoveAt(pick);   // 중복 방지
            }
            return _result;
        }

        /// <summary>이 강화가 지금 롤에 등장 가능한지. 무기 카드는 마일스톤에만·최대치 미도달·보유 무관, 스탯은 maxStacks 미도달.</summary>
        bool IsEligible(UpgradeDefinition d, bool milestone)
        {
            if (d == null || d.weight <= 0f) return false;
            if (TryWeaponId(d.type, out WeaponId id))
            {
                if (!milestone || shooter == null) return false;   // 무기 카드 = 마일스톤 전용
                return !shooter.IsWeaponMaxed(id);                 // Lv4 최대치 도달 무기 제외
            }
            return !(d.maxStacks > 0 && StackOf(d.type) >= d.maxStacks);
        }

        /// <summary>가중치 비례 인덱스 추첨(리스트 비었으면 마지막 인덱스 방어).</summary>
        static int PickWeighted(List<UpgradeDefinition> list)
        {
            float total = 0f;
            foreach (var d in list) total += d.weight;
            float r = Random.Range(0f, total);
            for (int i = 0; i < list.Count; i++)
            {
                r -= list[i].weight;
                if (r <= 0f) return i;
            }
            return list.Count - 1;
        }

        /// <summary>UpgradeType → WeaponId(무기 카드 여부). 무기 3종만 true.</summary>
        static bool TryWeaponId(UpgradeType t, out WeaponId id)
        {
            switch (t)
            {
                case UpgradeType.WeaponStraight: id = WeaponId.Straight; return true;
                case UpgradeType.WeaponHoming:   id = WeaponId.Homing;   return true;
                case UpgradeType.WeaponRailgun:  id = WeaponId.Railgun;  return true;
                default: id = default; return false;
            }
        }

        /// <summary>강화 적용(라우팅) — type별 적용 대상·방식이 갈림. 값은 SO(def.value). 팝업이 선택 확정 시 호출.</summary>
        public void Apply(UpgradeDefinition def)
        {
            if (def == null) return;
            if (TryWeaponId(def.type, out WeaponId wid))
            {
                shooter?.AcquireOrLevelUp(wid);   // 무기 카드: 레벨은 무기가 추적(_stacks 미사용)
                return;
            }
            switch (def.type)
            {
                case UpgradeType.MoveSpeed:    if (movement != null)   movement.AddMoveSpeedMultiplier(def.value); break;
                case UpgradeType.MaxHp:        if (health != null)     health.AddMaxHp(def.value);                 break;
                case UpgradeType.MagnetRadius: if (orbPool != null)    orbPool.AddMagnetRadius(def.value);         break;
                case UpgradeType.HpRegen:      if (health != null)     health.AddRegen(def.value);                 break;
                case UpgradeType.OrbValue:     if (experience != null) experience.AddOrbValueBonus(def.value);     break;
                case UpgradeType.AttackPower:  if (shooter != null)    shooter.AddAttackPower(def.value);          break;
                case UpgradeType.ShieldCooldown: if (shield != null)   shield.AddCooldownReduction(def.value);      break;
                case UpgradeType.ShieldDuration: if (shield != null)   shield.AddDuration(def.value);               break;
                case UpgradeType.ShieldHp:       if (shield != null)   shield.AddShieldHp(def.value);               break;
            }
            _stacks[def.type] = StackOf(def.type) + 1;   // 스택 누적(maxStacks 판정용)
        }

        /// <summary>카드 표시 데이터 — SO의 제목/설명/효과문자열(UI 하드코딩 회피).</summary>
        public UpgradeDisplay Describe(UpgradeDefinition def)
        {
            if (def == null) return new UpgradeDisplay(string.Empty, string.Empty, string.Empty);
            if (TryWeaponId(def.type, out WeaponId wid))
            {
                // 무기 카드: 효과문구를 보유 상태에서 파생(미보유=획득 / 보유=Lv업).
                int lv = shooter != null ? shooter.WeaponLevel(wid) : 0;
                string effect = lv <= 0 ? "획득 (Lv1)" : "Lv" + lv + " → Lv" + (lv + 1);
                return new UpgradeDisplay(def.title, def.description, effect);
            }
            return new UpgradeDisplay(def.title, def.description, def.EffectText);
        }
    }
}
