using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 난이도 페이즈 정의 SO (M4-5, progression-design §2). 한 인스턴스 = 시간축 한 구간.
    /// <see cref="DifficultyProvider"/>가 순서 배열로 들고, 경과시간에 따라 <b>전역 스탯 배율</b>(체력/속도/데미지)을 만든다.
    /// <para>페이즈 내부 = <see cref="startMultiplier"/>→<see cref="endMultiplier"/> 선형 <b>미세 상승</b>,
    /// 경계 = 다음 페이즈 <see cref="startMultiplier"/>로 <b>점프</b> + <see cref="bannerText"/> HUD 안내.
    /// 수치는 Day5(오서링 툴 편집). killScore·공격AI 필드는 배율 밖(<see cref="StatScaler"/>).</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Phase_", menuName = "Void Drift/Difficulty Phase")]
    public sealed class DifficultyPhaseDefinition : ScriptableObject
    {
        [Header("표시")]
        [Tooltip("페이즈 이름(툴/디버그 표시용). 로직 무관")]
        public string phaseName;

        [Header("구간")]
        [Tooltip("이 페이즈 지속시간(초). 경과시간이 이만큼 쌓이면 다음 페이즈로")]
        [Min(0.1f)] public float durationSeconds = 30f;

        [Header("전역 스탯 배율 (체력/속도/데미지)")]
        [Tooltip("페이즈 시작 시점 배율. 이전 페이즈 끝보다 크면 '경계 점프'")]
        [Min(0.01f)] public float startMultiplier = 1f;
        [Tooltip("페이즈 끝 시점 배율. 시작~끝 선형 보간 = 페이즈 내 미세 상승")]
        [Min(0.01f)] public float endMultiplier = 1f;

        [Header("경계 안내 (HUD)")]
        [Tooltip("이 페이즈 진입 시 표시할 문구. 비우면 배너 없음(예: 첫 페이즈)")]
        [TextArea] public string bannerText;

        [Header("스폰 프로파일 (M4-6)")]
        [Tooltip("이 페이즈 동안 쓸 스폰 프로파일(적 조합+밀도). 비우면 스포너 기본 표로 폴백")]
        public SpawnProfileDefinition spawnProfile;
    }
}
