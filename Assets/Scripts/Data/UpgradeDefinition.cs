using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 3choice 강화 정의 SO (M3-4). M1-8 하드코딩 수치를 데이터로 이관 — 에디터 오서링 창(Upgrade Authoring)이 편집.
    /// 한 인스턴스 = 강화 1종. <see cref="type"/>는 <see cref="UpgradeSystem"/>의 Apply 라우팅 키, 나머지는 수치·표시·롤 규칙 데이터.
    /// <para>효과 적용 방식(어떤 시스템의 어떤 메서드)은 type별로 코드가 결정 — SO는 값/문구/가중치/스택상한만 담는다.
    /// 수치는 Day5 튜닝. 무기별 파워(연사/탄속/관통)는 M4-8(무기 스코프).</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Void Drift/Upgrade Definition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [Header("종류(라우팅 키)")]
        [Tooltip("강화 종류. UpgradeSystem이 이 값으로 적용 대상·방식을 라우팅")]
        public UpgradeType type;

        [Header("표시(3choice 카드)")]
        [Tooltip("카드 제목")]
        public string title;
        [Tooltip("카드 설명")]
        [TextArea] public string description;

        [Header("효과")]
        [Tooltip("효과 수치. 의미는 type별 상이(배율 강화는 0.12=+12%, 가산 강화는 절대값)")]
        public float value;
        [Tooltip("표시 형식 — 체크 시 '+{value×100}%', 해제 시 '+{value}'")]
        public bool isPercent;

        [Header("롤 규칙")]
        [Tooltip("롤 가중치(등장 확률). 0이면 제외")]
        [Min(0f)] public float weight = 1f;
        [Tooltip("최대 스택(누적 상한). 0=무제한. 도달 시 롤 풀에서 제외")]
        [Min(0)] public int maxStacks = 0;

        /// <summary>카드 효과 표시 문자열(값/형식에서 파생 — UI 하드코딩 회피).</summary>
        public string EffectText => isPercent
            ? "+" + Mathf.RoundToInt(value * 100f) + "%"
            : "+" + value.ToString("0.##");
    }
}
