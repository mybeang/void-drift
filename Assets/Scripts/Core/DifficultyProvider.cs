using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 전역 난이도 → 적 스탯 배율 제공 (M2-5d, <b>스텁</b>). 적 빌더가 스폰 시 이 배율을 base 스탯에 곱해
    /// effective를 만든다(<see cref="StatScaler"/>). base(적 테이블 RO) · 배율(여기, 진행/난이도) · effective(런타임) 3층 분리.
    /// <para>⚠️ M2-5는 <b>배율 1.0 고정</b> — seam만. 실제 시간/페이즈 곡선(페이즈 내 미세 상승 + 경계 점프)은
    /// <b>M4-5</b>(progression-design §2)에서 이 자리를 채운다.</para>
    /// </summary>
    public sealed class DifficultyProvider : MonoBehaviour
    {
        /// <summary>현재 전역 스탯 배율(체력/속도/데미지에 적용). 스텁=1.0. M4-5에서 시간/페이즈로 대체.</summary>
        public float StatMultiplier => 1f;   // TODO(M4-5): 경과시간/페이즈 기반 곡선
    }
}
