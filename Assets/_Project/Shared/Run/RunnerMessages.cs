namespace RoadAndCode.NeonRush.Shared.Run
{
    public enum RunnerMove
    {
        ChangedLane,
        Jumped,
        Slid,
    }

    /// <summary>The runner did something the player asked for. Sound and effects react to this.</summary>
    public readonly struct RunnerMoved
    {
        public readonly RunnerMove Move;

        public RunnerMoved(RunnerMove move)
        {
            Move = move;
        }
    }
}
