namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>How numbers are sized across the screens, so the same score looks the same everywhere.</summary>
    internal static class ScoreDisplay
    {
        /// <summary>Room for 9,999,999. A longer run than that shows as all nines rather than wrapping.</summary>
        public const int Digits = 7;

        public const int MultiplierDigits = 2;
    }
}
