using System;
using System.Collections.Generic;
using UnityEngine;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 3choice 강화 풀·적용·표시데이터 (M1-8 데이터/로직). 팝업 UI(M1-7 <c>LevelUpPopup</c>)가 구동한다.
    /// 공용 스탯 3종 = 이동속도/최대체력/자석범위(<see cref="UpgradeType"/>). 효과는 능력치별 상이(이동=배율%, 체력·자석=가산).
    /// <see cref="Roll"/>/<see cref="Apply"/>/<see cref="Describe"/>는 public — 팝업이 사용. 표시 수치는 이 필드에서 나옴(UI 하드코딩 회피). 수치 Day5.
    /// </summary>
    public sealed class UpgradeSystem : MonoBehaviour
    {
        [Header("참조(비우면 씬 자동 탐색)")]
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerHealth health;
        [SerializeField] OrbPool orbPool;

        [Header("효과 수치 (능력치별 상이, Day5)")]
        [Tooltip("이동속도: 드래그 게인 배율 증가율. 0.12 → +12%(누적)")]
        [SerializeField] float moveSpeedPct = 0.12f;
        [Tooltip("최대체력: 가산량(늘린 만큼 회복)")]
        [SerializeField] float maxHpAdd = 20f;
        [Tooltip("자석범위: 가산량(월드 유닛)")]
        [SerializeField] float magnetRadiusAdd = 2f;

        static readonly UpgradeType[] All = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));
        readonly List<UpgradeType> _buffer = new List<UpgradeType>();

        void Awake()
        {
            if (movement == null) movement = FindAnyObjectByType<PlayerMovement>();
            if (health == null) health = FindAnyObjectByType<PlayerHealth>();
            if (orbPool == null) orbPool = FindAnyObjectByType<OrbPool>();
        }

        /// <summary>풀에서 중복 없이 최대 count개 무작위 롤(항목 부족 시 있는 만큼).</summary>
        public List<UpgradeType> Roll(int count)
        {
            _buffer.Clear();
            _buffer.AddRange(All);
            int n = Mathf.Min(count, _buffer.Count);
            for (int i = 0; i < n; i++)   // Fisher–Yates 부분 셔플
            {
                int j = UnityEngine.Random.Range(i, _buffer.Count);
                (_buffer[i], _buffer[j]) = (_buffer[j], _buffer[i]);
            }
            return _buffer.GetRange(0, n);
        }

        /// <summary>강화 적용(라우팅) — 능력치별 적용 방식 상이. 팝업이 선택 확정 시 호출.</summary>
        public void Apply(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.MoveSpeed:
                    if (movement != null) movement.AddMoveSpeedMultiplier(moveSpeedPct);
                    break;
                case UpgradeType.MaxHp:
                    if (health != null) health.AddMaxHp(maxHpAdd);
                    break;
                case UpgradeType.MagnetRadius:
                    if (orbPool != null) orbPool.AddMagnetRadius(magnetRadiusAdd);
                    break;
            }
        }

        /// <summary>카드 표시 데이터 — 제목/설명 + <b>실제 수치 필드에서 포맷한 효과</b>(UI 하드코딩 회피).</summary>
        public UpgradeDisplay Describe(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.MoveSpeed:
                    return new UpgradeDisplay("이동속도", "기체 이동속도 증가", "+" + Mathf.RoundToInt(moveSpeedPct * 100f) + "%");
                case UpgradeType.MaxHp:
                    return new UpgradeDisplay("최대 체력", "최대 HP 증가 (즉시 회복)", "+" + Mathf.RoundToInt(maxHpAdd));
                case UpgradeType.MagnetRadius:
                    return new UpgradeDisplay("자석 범위", "오브 획득 반경 증가", "+" + magnetRadiusAdd.ToString("0.#"));
                default:
                    return new UpgradeDisplay(type.ToString(), "", "");
            }
        }
    }
}
