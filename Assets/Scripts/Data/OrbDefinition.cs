using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 경험치 오브 종류 정의 SO (M2-2c). 적이 <b>어떤 오브를 떨구나</b>를 데이터로 정의 —
    /// <see cref="EnemyDefinition.dropOrb"/>가 이 SO를 참조한다(적은 오브 1개 고정 드랍).
    /// 오브 종류를 가르는 것 = <see cref="xpValue"/>(습득 경험치) + <see cref="visual"/>(색만 다른 크리스탈).
    /// <para>동작(자석/습득)은 <b>공유 <see cref="Orb"/> 하나</b>가 담당하고, 종류별로 다른 건 비주얼뿐 —
    /// 드랍 시 공유 Orb에 이 <see cref="visual"/>을 주입하는 방식(결정 (a), DRY). 크리스탈 3종은 사용자가 분리해 둠(green/blue/red).</para>
    /// <para>⚠️ 런타임 배선(오브가 이 정의의 <see cref="xpValue"/>로 경험치 발행 / 드랍 측이 <see cref="visual"/> 주입)은
    /// 이후 단계 — 지금은 데이터 스키마만. 현재 <c>Orb.xpValue</c>·단일 비주얼은 유지(대체는 드랍 데이터화 시).</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Orb_", menuName = "Void Drift/Orb Definition")]
    public sealed class OrbDefinition : ScriptableObject
    {
        [Tooltip("습득 시 주는 경험치. 오브 종류를 가르는 값. 밸런싱 패스 3-5: 녹2·빨3·파4(비율 1:1.5:2). 튜닝값")]
        public int xpValue = 2;

        [Tooltip("이 오브 종류의 비주얼 프리팹(크리스탈 VFX, 색만 다름). 동작은 공유 Orb가 담당")]
        public GameObject visual;

        [Header("웨이브 색 드랍 (3-5, 데이터 게이팅)")]
        [Tooltip("이 색이 출현하기 시작하는 웨이브. 녹0·빨30·파50. 현재 웨이브 < 이 값이면 롤에서 제외")]
        [Min(0)] public int minWave = 0;

        [Tooltip("드랍 가중치(가능한 색끼리 재정규화). 녹5·빨3·파1")]
        [Min(0f)] public float weight = 5f;
    }
}
