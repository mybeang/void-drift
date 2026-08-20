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

        readonly Dictionary<UpgradeType, int> _stacks = new Dictionary<UpgradeType, int>();
        readonly List<UpgradeDefinition> _candidates = new List<UpgradeDefinition>();
        readonly List<UpgradeDefinition> _result = new List<UpgradeDefinition>();

        void Awake()
        {
            if (movement == null) movement = FindAnyObjectByType<PlayerMovement>();
            if (health == null) health = FindAnyObjectByType<PlayerHealth>();
            if (orbPool == null) orbPool = FindAnyObjectByType<OrbPool>();
            if (shooter == null) shooter = FindAnyObjectByType<PlayerShooter>();
            if (experience == null) experience = FindAnyObjectByType<ExperienceSystem>();
        }

        int StackOf(UpgradeType t) => _stacks.TryGetValue(t, out int n) ? n : 0;

        /// <summary>가중치 기반 중복없는 롤(최대 count개). weight 0·maxStacks 도달 종은 제외.</summary>
        public List<UpgradeDefinition> Roll(int count)
        {
            _candidates.Clear();
            foreach (var d in pool)
            {
                if (d == null || d.weight <= 0f) continue;
                if (d.maxStacks > 0 && StackOf(d.type) >= d.maxStacks) continue;
                _candidates.Add(d);
            }

            _result.Clear();
            int n = Mathf.Min(count, _candidates.Count);
            for (int k = 0; k < n; k++)
            {
                float total = 0f;
                foreach (var d in _candidates) total += d.weight;

                float r = Random.Range(0f, total);
                int pick = _candidates.Count - 1;
                for (int i = 0; i < _candidates.Count; i++)
                {
                    r -= _candidates[i].weight;
                    if (r <= 0f) { pick = i; break; }
                }
                _result.Add(_candidates[pick]);
                _candidates.RemoveAt(pick);   // 중복 방지
            }
            return _result;
        }

        /// <summary>강화 적용(라우팅) — type별 적용 대상·방식이 갈림. 값은 SO(def.value). 팝업이 선택 확정 시 호출.</summary>
        public void Apply(UpgradeDefinition def)
        {
            if (def == null) return;
            switch (def.type)
            {
                case UpgradeType.MoveSpeed:    if (movement != null)   movement.AddMoveSpeedMultiplier(def.value); break;
                case UpgradeType.MaxHp:        if (health != null)     health.AddMaxHp(def.value);                 break;
                case UpgradeType.MagnetRadius: if (orbPool != null)    orbPool.AddMagnetRadius(def.value);         break;
                case UpgradeType.HpRegen:      if (health != null)     health.AddRegen(def.value);                 break;
                case UpgradeType.OrbValue:     if (experience != null) experience.AddOrbValueBonus(def.value);     break;
                case UpgradeType.AttackPower:  if (shooter != null)    shooter.AddAttackPower(def.value);          break;
            }
            _stacks[def.type] = StackOf(def.type) + 1;   // 스택 누적(maxStacks 판정용)
        }

        /// <summary>카드 표시 데이터 — SO의 제목/설명/효과문자열(UI 하드코딩 회피).</summary>
        public UpgradeDisplay Describe(UpgradeDefinition def)
        {
            if (def == null) return new UpgradeDisplay(string.Empty, string.Empty, string.Empty);
            return new UpgradeDisplay(def.title, def.description, def.EffectText);
        }
    }
}
