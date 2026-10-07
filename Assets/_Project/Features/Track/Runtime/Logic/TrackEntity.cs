using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>One thing currently on the track. Instances are recycled, never thrown away.</summary>
    internal sealed class TrackEntity
    {
        public TrackEntity(int id)
        {
            Id = id;
        }

        /// <summary>Stable for the lifetime of the instance, so presentation can index by it.</summary>
        public int Id { get; }

        public ITrackEntityDefinition Definition { get; private set; }

        public float X { get; private set; }

        /// <summary>Distance ahead of the runner. Negative once it has gone past.</summary>
        public float Z { get; private set; }

        /// <summary>Set after the first contact, so one hazard is one hit.</summary>
        public bool Touched { get; private set; }

        public Bounds Bounds => Definition.BoundsAt(X, Z);

        public void Place(ITrackEntityDefinition definition, float x, float z)
        {
            Definition = definition;
            X = x;
            Z = z;
            Touched = false;
        }

        public void Advance(float distance) => Z -= distance;

        public void MarkTouched() => Touched = true;
    }
}
