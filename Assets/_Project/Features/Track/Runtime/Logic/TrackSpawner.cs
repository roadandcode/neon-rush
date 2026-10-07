using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Data;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>
    /// Decides what appears on the track and when. Rows enter at the spawn line, spaced by travel
    /// time, so a faster run gets more room between rows rather than less time to react.
    /// </summary>
    internal sealed class TrackSpawner
    {
        private readonly TrackField _field;
        private readonly IPatternPicker _picker;
        private readonly ILaneLayout _lanes;
        private readonly TrackSettings _settings;

        private ITrackPattern _pattern;
        private int _row;
        private float _untilNextRow;

        public TrackSpawner(TrackField field, IPatternPicker picker, ILaneLayout lanes, TrackSettings settings)
        {
            _field = Guard.NotNull(field, nameof(field));
            _picker = Guard.NotNull(picker, nameof(picker));
            _lanes = Guard.NotNull(lanes, nameof(lanes));
            _settings = Guard.NotNull(settings, nameof(settings));
            Reset();
        }

        public void Reset()
        {
            _picker.Reset();
            _pattern = null;
            _row = 0;
            _untilNextRow = _settings.StartDelayDistance;
        }

        /// <param name="distance">Metres travelled this tick.</param>
        /// <param name="speed">Current speed, used to space the rows that follow.</param>
        /// <param name="tier">Current difficulty tier, used to choose patterns.</param>
        public void Advance(float distance, float speed, int tier)
        {
            _untilNextRow -= distance;

            while (_untilNextRow <= 0f)
            {
                if (_pattern == null || _row >= _pattern.RowCount)
                {
                    _pattern = _picker.Next(tier);
                    _row = 0;
                }

                // A tick usually overshoots the moment a row was due. Placing the row that much
                // closer keeps the spacing exact whatever the frame rate.
                SpawnRow(_pattern, _row, _settings.SpawnDistance + _untilNextRow);
                _row++;

                bool patternFinished = _row >= _pattern.RowCount;
                float seconds = patternFinished ? _settings.PatternGapSeconds : _settings.RowGapSeconds;
                _untilNextRow += Mathf.Max(_settings.MinRowGap, speed * seconds);
            }
        }

        private void SpawnRow(ITrackPattern pattern, int row, float z)
        {
            for (int lane = 0; lane < _lanes.LaneCount; lane++)
            {
                var definition = pattern.CellAt(row, lane);
                if (definition != null) _field.Spawn(definition, _lanes.CenterOf(lane), z);
            }
        }
    }
}
