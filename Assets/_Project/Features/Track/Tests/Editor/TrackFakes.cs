using System.Collections.Generic;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Randomness;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Tests
{
    /// <summary>A track entity defined in code: a box, and what touching it does.</summary>
    internal sealed class FakeDefinition : ITrackEntityDefinition
    {
        private readonly Vector3 _size;
        private readonly float _bottom;
        private readonly int? _pickupValue;

        private FakeDefinition(string name, Vector3 size, float bottom, int? pickupValue)
        {
            Name = name;
            _size = size;
            _bottom = bottom;
            _pickupValue = pickupValue;
        }

        public string Name { get; }

        public int Touches { get; private set; }

        public static FakeDefinition Hazard(string name, float bottom = 0f, float height = 1f)
        {
            return new FakeDefinition(name, new Vector3(1f, height, 1f), bottom, null);
        }

        public static FakeDefinition Pickup(string name, int value)
        {
            return new FakeDefinition(name, Vector3.one, 0f, value);
        }

        public Bounds BoundsAt(float x, float z)
        {
            return new Bounds(new Vector3(x, _bottom + _size.y * 0.5f, z), _size);
        }

        public bool OnTouched(IPublisher publisher)
        {
            Touches++;
            if (_pickupValue.HasValue)
            {
                publisher.Publish(new PickupCollected(_pickupValue.Value));
                return true;
            }

            publisher.Publish(new HazardHit());
            return false;
        }

        public override string ToString() => Name;
    }

    /// <summary>A pattern written as rows of cells, left lane first. Null is an empty cell.</summary>
    internal sealed class FakePattern : ITrackPattern
    {
        private readonly ITrackEntityDefinition[][] _rows;

        public FakePattern(string name, int minTier, float weight, params ITrackEntityDefinition[][] rows)
        {
            Name = name;
            MinTier = minTier;
            Weight = weight;
            _rows = rows;
        }

        public string Name { get; }

        public int MinTier { get; }

        public float Weight { get; }

        public int RowCount => _rows.Length;

        public ITrackEntityDefinition CellAt(int row, int lane)
        {
            if (row < 0 || row >= _rows.Length) return null;
            return lane >= 0 && lane < _rows[row].Length ? _rows[row][lane] : null;
        }

        public override string ToString() => Name;
    }

    /// <summary>Always hands out the same pattern.</summary>
    internal sealed class SinglePatternPicker : IPatternPicker
    {
        private readonly ITrackPattern _pattern;

        public SinglePatternPicker(ITrackPattern pattern)
        {
            _pattern = pattern;
        }

        public int Resets { get; private set; }

        public ITrackPattern Next(int tier) => _pattern;

        public void Reset() => Resets++;
    }

    internal sealed class FixedProgress : IRunProgress
    {
        public float Elapsed { get; set; }

        public float Distance { get; set; }

        public float Speed { get; set; }

        public int Tier { get; set; }
    }

    internal sealed class FixedBody : IRunnerBody
    {
        public Bounds Bounds { get; set; } = new Bounds(new Vector3(0f, 0.9f, 0f), new Vector3(0.9f, 1.8f, 0.9f));
    }

    /// <summary>Replays a fixed list of rolls, then keeps returning the last one.</summary>
    internal sealed class ScriptedRandom : IRandom
    {
        private readonly Queue<float> _rolls;
        private float _last;

        public ScriptedRandom(params float[] rolls)
        {
            _rolls = new Queue<float>(rolls);
        }

        public float NextFloat()
        {
            if (_rolls.Count > 0) _last = _rolls.Dequeue();
            return _last;
        }

        public int NextInt(int minInclusive, int maxExclusive) => minInclusive;
    }
}
