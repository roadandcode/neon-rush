using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Data
{
    /// <summary>
    /// Spacing is set in seconds of travel, not metres, so the time a player has to react
    /// stays the same as the run speeds up.
    /// </summary>
    [Serializable]
    internal sealed class TrackSettings
    {
        [Tooltip("How far ahead of the runner new rows appear, in metres.")]
        [SerializeField, Min(1f)] private float _spawnDistance = 85f;

        [Tooltip("How far behind the runner rows are recycled, in metres.")]
        [SerializeField, Min(0f)] private float _despawnDistance = 8f;

        [SerializeField, Min(0.05f)] private float _rowGapSeconds = 0.55f;
        [SerializeField, Min(0.05f)] private float _patternGapSeconds = 0.95f;

        [Tooltip("Rows are never closer than this, however slow the run is.")]
        [SerializeField, Min(0.5f)] private float _minRowGap = 6f;

        [Tooltip("Empty track at the start of a run, in metres.")]
        [SerializeField, Min(0f)] private float _startDelayDistance = 20f;

        public float SpawnDistance => _spawnDistance;
        public float DespawnDistance => _despawnDistance;
        public float RowGapSeconds => _rowGapSeconds;
        public float PatternGapSeconds => _patternGapSeconds;
        public float MinRowGap => _minRowGap;
        public float StartDelayDistance => _startDelayDistance;
    }
}
