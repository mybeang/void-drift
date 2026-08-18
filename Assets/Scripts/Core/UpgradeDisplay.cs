namespace VD.Core
{
    /// <summary>
    /// 3choice 카드 표시 데이터 (M1-7). 제목/설명/효과수치 문자열.
    /// 값은 UI가 하드코딩하지 않고 <c>UpgradeSystem.Describe</c>가 실제 수치 필드에서 채운다(→ M2-2 SO로 이관 가능).
    /// </summary>
    public readonly struct UpgradeDisplay
    {
        public readonly string Title;
        public readonly string Description;
        public readonly string Effect;

        public UpgradeDisplay(string title, string description, string effect)
        {
            Title = title;
            Description = description;
            Effect = effect;
        }
    }
}
