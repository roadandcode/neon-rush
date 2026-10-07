namespace RoadAndCode.NeonRush.Shared.Track
{
    /// <summary>Where the lanes are. Lanes are numbered from the left, starting at zero.</summary>
    public interface ILaneLayout
    {
        int LaneCount { get; }
        int CenterLane { get; }
        float LaneWidth { get; }

        bool Contains(int lane);

        /// <summary>World X of the middle of a lane.</summary>
        float CenterOf(int lane);
    }
}
