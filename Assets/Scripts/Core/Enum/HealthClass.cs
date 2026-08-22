namespace VD.Core
{
    /// <summary>
    /// 적 체력 등급 (밸런싱 패스 3-2·3-3). HP 곡선(저/고)·속도 체력배율·모델 스케일·스폰 가중을 일괄 결정.
    /// HP 실제 수치는 웨이브 곡선(<see cref="WaveDifficultyConfig"/>)에서 등급별로 나온다(3-2).
    /// </summary>
    public enum HealthClass
    {
        Low,   // 저체력 — HP 곡선 LowHp, 모델 ×0.8, 속도 체력배율 1.0
        High,  // 고체력 — HP 곡선 HighHp(wave 40~), 모델 ×1.2, 속도 체력배율 0.6
    }
}
