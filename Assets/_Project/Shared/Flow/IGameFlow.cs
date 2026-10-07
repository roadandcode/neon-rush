namespace RoadAndCode.NeonRush.Shared.Flow
{
    public enum GamePhase
    {
        Boot,
        Menu,
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

        /// <summary>Starts a fresh run from the menu or the game-over screen.</summary>
        bool StartRun();

        bool Pause();

        bool Resume();

        /// <summary>The player crashed. Ends the current run and shows game over.</summary>
        bool FailRun();

        /// <summary>Back to the menu from game over, or from pause (which abandons the run).</summary>
        bool ReturnToMenu();
    }
}
