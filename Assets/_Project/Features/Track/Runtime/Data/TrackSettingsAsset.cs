using System.Collections.Generic;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Data
{
    [CreateAssetMenu(menuName = "Neon Rush/Track/Settings", fileName = "TrackSettings")]
    internal sealed class TrackSettingsAsset : ScriptableObject
    {
        [SerializeField] private TrackSettings _settings = new TrackSettings();
        [SerializeField] private TrackPatternAsset[] _patterns = new TrackPatternAsset[0];

        public TrackSettings Settings => _settings;

        public IReadOnlyList<ITrackPattern> Patterns => _patterns;

        private void OnValidate()
        {
            if (_patterns.Length == 0) Debug.LogError($"{name}: the track needs at least one pattern.", this);

            foreach (var pattern in _patterns)
            {
                if (pattern == null) Debug.LogError($"{name}: the pattern list has an empty slot.", this);
            }
        }
    }
}
