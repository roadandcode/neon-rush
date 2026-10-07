namespace RoadAndCode.NeonRush.Shared.Flow
{
    public enum RunEndReason
    {
        Crashed,
        Abandoned,
    }

    /// <summary>The game moved from one phase to another. UI and audio switch on this.</summary>
    public readonly struct GamePhaseChanged
    {
        public readonly GamePhase Previous;
        public readonly GamePhase Current;

        public GamePhaseChanged(GamePhase previous, GamePhase current)
        {
            Previous = previous;
            Current = current;
        }
    }

    /// <summary>
    /// A new run is about to begin. Sent before the simulation starts ticking, so systems
    /// reset here. Everything random in the run derives from <see cref="Seed"/>.
    /// </summary>
    public readonly struct RunStarted
    {
        public readonly int Seed;

        public RunStarted(int seed)
        {
            Seed = seed;
        }
    }

    /// <summary>The run is over and the simulation has stopped.</summary>
    public readonly struct RunEnded
    {
        public readonly RunEndReason Reason;

        public RunEnded(RunEndReason reason)
        {
            Reason = reason;
        }
    }
}
