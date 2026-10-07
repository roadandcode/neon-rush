using System;

namespace RoadAndCode.NeonRush.Shared.Track
{
    /// <summary>Evenly spaced lanes, centred on x = 0.</summary>
    public sealed class LaneGrid : ILaneLayout
    {
        public LaneGrid(int laneCount, float laneWidth)
        {
            if (laneCount < 1) throw new ArgumentOutOfRangeException(nameof(laneCount));
            if (laneWidth <= 0f) throw new ArgumentOutOfRangeException(nameof(laneWidth));

            LaneCount = laneCount;
            LaneWidth = laneWidth;
        }

        public int LaneCount { get; }

        public float LaneWidth { get; }

        public int CenterLane => LaneCount / 2;

        public bool Contains(int lane) => lane >= 0 && lane < LaneCount;

        public float CenterOf(int lane) => (lane - (LaneCount - 1) * 0.5f) * LaneWidth;
    }
}
