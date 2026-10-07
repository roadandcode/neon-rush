namespace RoadAndCode.NeonRush.Pace.Logic
{
    /// <summary>How the run gets harder over time. The asset implements it; tests use their own.</summary>
    internal interface IDifficultyCurve
    {
        /// <summary>Speed in metres per second after <paramref name="elapsed"/> seconds.</summary>
        float SpeedAt(float elapsed);

        /// <summary>Difficulty tier after <paramref name="elapsed"/> seconds, starting at zero.</summary>
        int TierAt(float elapsed);
    }
}
