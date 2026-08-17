namespace VD.Core
{
    /// <summary>
    /// GameScene 내부의 게임 진행 상태. Title/Result는 별도 씬이므로 이 FSM에 포함하지 않는다.
    /// Boot(로딩 페이즈) → Playing → Paused(3choice 등) → GameOver.
    /// </summary>
    public enum GameState
    {
        Boot,
        Playing,
        Paused,
        GameOver,
    }
}
