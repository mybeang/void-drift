using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VD.Core
{
    /// <summary>
    /// 적 조합 정의 SO (enemy-design §2·§6, M2-2c). 에디터 오서링 툴(M2-3)이 편집하는 <b>디자인 원천 데이터</b>.
    /// 한 인스턴스 = 비주얼(AssetReference) × <see cref="MoveAIType"/> × <see cref="AttackAIType"/> × <see cref="Archetype"/> × <see cref="EnemyStats"/>의 한 조합.
    /// <para>range(교전거리)는 <b>저장 필드가 아님</b> — <see cref="Archetype"/>에서 파생(단일 소스, 결정 #4 · <see cref="RangeLabelOf"/>).
    /// 실제 이동/공격 로직·스폰 배선은 M3-1/M3-2/M2-5. 수치는 Day5.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_", menuName = "Void Drift/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("비주얼")]
        [Tooltip("적 비주얼 프리팹(Addressables 'Enemy' 그룹의 Enemy_* 중 하나). 스폰 시 로드(M2-5)")]
        public AssetReferenceGameObject visual;

        [Tooltip("비주얼 크기 배수 — 모델별 크기 편차 보정(M3-3). 셸 자식 비주얼의 로컬 스케일에 곱함. 1=원본, 0이하는 1로 취급. 히트박스(셸 콜라이더)는 별도 고정")]
        public float visualScale = 1f;

        [Header("AI 조합")]
        [Tooltip("이동 AI 종류. 실제 이동 로직은 M3-1")]
        public MoveAIType moveAI;

        [Tooltip("공격 AI 종류. 실제 공격 로직은 M3-2")]
        public AttackAIType attackAI;

        [Tooltip("아키타입. 교전거리(range)는 여기서 파생 — 별도 필드로 두지 않음(단일 소스)")]
        public Archetype archetype;

        [Header("스탯")]
        [Tooltip("능력치(체력/이동속도/데미지/처치점수 + 공격AI별). 수치 Day5")]
        public EnemyStats stats;

        [Header("드랍")]
        [Tooltip("실사망 시 떨굴 오브 종류(1개 고정). xp 값은 OrbDefinition측에 정의. 드랍 실배선은 이후 단계")]
        public OrbDefinition dropOrb;

        /// <summary>이 정의의 파생 교전거리 라벨(<see cref="archetype"/> 기준).</summary>
        public string RangeLabel => RangeLabelOf(archetype);

        /// <summary>
        /// 아키타입 → 교전거리(range) 라벨 파생. <b>단일 소스</b>(결정 #4). Addressables <c>range:</c> 라벨과 동일 문자열
        /// (탄막=원거리 · 돌진/자폭=근거리 · 복합=복합) — M2-4 유효성·라벨 교차검증의 기준.
        /// </summary>
        public static string RangeLabelOf(Archetype archetype) => archetype switch
        {
            Archetype.Shooter => "원거리",
            Archetype.Charger => "근거리",
            Archetype.Bomber => "근거리",
            Archetype.Hybrid => "복합",
            _ => "복합",
        };
    }
}
