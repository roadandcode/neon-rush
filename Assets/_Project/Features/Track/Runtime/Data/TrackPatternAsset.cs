using System;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Data
{
    [CreateAssetMenu(menuName = "Neon Rush/Track/Pattern", fileName = "Pattern")]
    internal sealed class TrackPatternAsset : ScriptableObject, ITrackPattern
    {
        [Serializable]
        private struct Row
        {
            [Tooltip("One slot per lane, left to right. Leave a slot empty for open track.")]
            [SerializeField] private TrackEntityDefinition[] _cells;

            public TrackEntityDefinition CellAt(int lane)
            {
                return _cells != null && lane >= 0 && lane < _cells.Length ? _cells[lane] : null;
            }
        }

        [SerializeField, Min(0)] private int _minTier;
        [SerializeField, Min(0.01f)] private float _weight = 1f;
        [SerializeField] private Row[] _rows = new Row[0];

        public int MinTier => _minTier;

        public float Weight => _weight;

        public int RowCount => _rows.Length;

        public ITrackEntityDefinition CellAt(int row, int lane)
        {
            if (row < 0 || row >= _rows.Length) return null;

            // An unassigned slot holds a destroyed-object reference that only Unity's own
            // comparison treats as null. Turn it into a real null before it leaves as an interface.
            var cell = _rows[row].CellAt(lane);
            return cell != null ? cell : null;
        }

        private void OnValidate()
        {
            if (_rows.Length == 0) Debug.LogError($"{name}: a pattern needs at least one row.", this);
        }
    }
}
