namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>A short authored sequence of rows. Each row has one cell per lane, possibly empty.</summary>
    internal interface ITrackPattern
    {
        /// <summary>The pattern only appears once the run has reached this difficulty tier.</summary>
        int MinTier { get; }

        /// <summary>Relative chance of being picked among the eligible patterns.</summary>
        float Weight { get; }

        int RowCount { get; }

        /// <summary>The entity in a cell, or null if the cell is empty or outside the pattern.</summary>
        ITrackEntityDefinition CellAt(int row, int lane);
    }
}
