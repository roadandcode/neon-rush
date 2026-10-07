using System;
using System.Collections.Generic;

namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>
    /// Everything on the track right now. Presentation listens to <see cref="Added"/> and
    /// <see cref="Removed"/> instead of the logic knowing that views exist.
    /// </summary>
    internal sealed class TrackField
    {
        private readonly List<TrackEntity> _active = new List<TrackEntity>();
        private readonly Stack<TrackEntity> _free = new Stack<TrackEntity>();
        private int _created;

        public event Action<TrackEntity> Added;

        public event Action<TrackEntity> Removed;

        public IReadOnlyList<TrackEntity> Entities => _active;

        /// <summary>How many entity instances exist in total, live or pooled.</summary>
        public int Capacity => _created;

        public TrackEntity Spawn(ITrackEntityDefinition definition, float x, float z)
        {
            var entity = _free.Count > 0 ? _free.Pop() : new TrackEntity(_created++);
            entity.Place(definition, x, z);
            _active.Add(entity);
            Added?.Invoke(entity);
            return entity;
        }

        /// <summary>Order is not preserved: the last entity takes the freed slot.</summary>
        public void RemoveAt(int index)
        {
            var entity = _active[index];
            int last = _active.Count - 1;
            _active[index] = _active[last];
            _active.RemoveAt(last);

            _free.Push(entity);
            Removed?.Invoke(entity);
        }

        public void Clear()
        {
            for (int i = _active.Count - 1; i >= 0; i--) RemoveAt(i);
        }
    }
}
