namespace RoadAndCode.NeonRush.Shared.Flow
{
    public enum GamePhase
    {
        Boot,
        Menu,

        /// <summary>The opening shot between pressing play and the run starting. Nothing is simulated yet.</summary>
        Intro,
        Run,
        Paused,
        GameOver,
    }

    /// <summary>
    /// The one owner of "which phase is the game in". Features ask it to change phase;
    /// each request returns false when it isn't valid from the current phase.
    /// </summary>
    public interface IGameFlow
    {
        GamePhase Phase { get; }

        /// <summary>
        /// Starts a fresh run. From the menu the run opens with its intro; from the game-over
        /// screen it starts at once, because a retry should not make the player wait.
        /// </summary>
        bool StartRun();

        bool Pause();

        bool Resume();

        /// <summary>The player crashed. Ends the current run and shows game over.</summary>
        bool FailRun();

        /// <summary>Back to the menu from game over, or from pause (which abandons the run).</summary>
        bool ReturnToMenu();
    }
}
