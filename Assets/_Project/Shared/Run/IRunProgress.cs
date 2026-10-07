namespace RoadAndCode.NeonRush.Shared.Run
{
    /// <summary>How far along the current run is. Pace owns it; everything else only reads.</summary>
    public interface IRunProgress
    {
        /// <summary>Seconds of simulated time since the run started.</summary>
        float Elapsed { get; }

        /// <summary>Metres travelled.</summary>
        float Distance { get; }

        /// <summary>Metres per second right now.</summary>
        float Speed { get; }

        /// <summary>Difficulty tier, starting at zero. Content unlocks by tier.</summary>
        int Tier { get; }
    }
}
