namespace RoadAndCode.NeonRush.Shared.Scoring
{
    /// <summary>The best score saved so far. Scoring owns it; screens read it.</summary>
    public interface IBestScore
    {
        int Best { get; }
    }
}
