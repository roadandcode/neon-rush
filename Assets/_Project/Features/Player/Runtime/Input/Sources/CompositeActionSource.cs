using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Player.Logic;

namespace RoadAndCode.NeonRush.Player.Input
{
    /// <summary>
    /// Several sources presented as one, so the controller never knows how many kinds of input
    /// the current platform has.
    /// </summary>
    internal sealed class CompositeActionSource : IPlayerActionSource, IDisposable
    {
        private readonly IPlayerActionSource[] _sources;

        public CompositeActionSource(params IPlayerActionSource[] sources)
        {
            _sources = Guard.NotNull(sources, nameof(sources));
        }

        public int SourceCount => _sources.Length;

        public void SetEnabled(bool enabled)
        {
            for (int i = 0; i < _sources.Length; i++) _sources[i].SetEnabled(enabled);
        }

        public bool TryDequeue(out PlayerAction action)
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i].TryDequeue(out action)) return true;
            }

            action = default;
            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < _sources.Length; i++) (_sources[i] as IDisposable)?.Dispose();
        }
    }
}
