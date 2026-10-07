namespace RoadAndCode.Core.Platforms
{
    /// <summary>Screen size behind an interface, so gesture thresholds can be tested at any resolution.</summary>
    public interface IScreenMetrics
    {
        /// <summary>The shorter side of the screen in pixels.</summary>
        float ShortSide { get; }
    }
}
