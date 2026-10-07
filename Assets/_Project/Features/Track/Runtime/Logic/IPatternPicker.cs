namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>Chooses which pattern comes next. A different picker is a different kind of run.</summary>
    internal interface IPatternPicker
    {
        ITrackPattern Next(int tier);

        /// <summary>Forgets what was picked before, for the start of a run.</summary>
        void Reset();
    }
}
