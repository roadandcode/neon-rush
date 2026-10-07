namespace RoadAndCode.NeonRush.Shared.Scoring
{
    /// <summary>Sent only when the visible score or multiplier actually changes.</summary>
    public readonly struct ScoreChanged
    {
        public readonly int Score;
        public readonly int Multiplier;

        public ScoreChanged(int score, int multiplier)
        {
            Score = score;
            Multiplier = multiplier;
        }
    }

    /// <summary>The final result of a run, after the best score has been updated.</summary>
    public readonly struct RunScored
    {
        public readonly int Score;
        public readonly int Best;
        public readonly bool IsNewBest;

        public RunScored(int score, int best, bool isNewBest)
        {
            Score = score;
            Best = best;
            IsNewBest = isNewBest;
        }
    }
}
