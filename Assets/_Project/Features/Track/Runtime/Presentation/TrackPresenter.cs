using System;
using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Pooling;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Track.Data;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Presentation
{
    /// <summary>
    /// Keeps one pooled view in step with each entity on the track, and scrolls the ground.
    /// Ticks after the simulation, so what is drawn is what was just simulated.
    /// </summary>
    internal sealed class TrackPresenter : ISimulationSystem, IDisposable
    {
        private const int PrewarmPerPrefab = 12;

        private readonly TrackField _field;
        private readonly IRunProgress _progress;
        private readonly GroundView _ground;
        private readonly Transform _entityRoot;
        private readonly Dictionary<TrackEntityView, ComponentPool<TrackEntityView>> _pools =
            new Dictionary<TrackEntityView, ComponentPool<TrackEntityView>>();

        // Indexed by TrackEntity.Id, which is stable while an entity instance is recycled.
        private TrackEntityView[] _views = new TrackEntityView[64];
        private TrackEntityView[] _prefabs = new TrackEntityView[64];
        private float[] _spinRates = new float[64];

        public TrackPresenter(TrackField field, IRunProgress progress, GroundView ground, Transform entityRoot)
        {
            _field = Guard.NotNull(field, nameof(field));
            _progress = Guard.NotNull(progress, nameof(progress));
            _ground = Guard.NotNull(ground, nameof(ground));
            _entityRoot = Guard.NotNull(entityRoot, nameof(entityRoot));

            _field.Added += OnAdded;
            _field.Removed += OnRemoved;
        }

        public void Tick(float deltaTime)
        {
            _ground.SetDistance(_progress.Distance);

            var entities = _field.Entities;
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var view = _views[entity.Id];

                // Plain reference check: Unity's == on a component costs a native call, and this runs per entity per frame.
                if (!ReferenceEquals(view, null)) view.Place(entity.X, entity.Z, _spinRates[entity.Id] * _progress.Elapsed);
            }
        }

        public void Dispose()
        {
            _field.Added -= OnAdded;
            _field.Removed -= OnRemoved;
            foreach (var pool in _pools.Values) pool.Dispose();
            _pools.Clear();
        }

        private void OnAdded(TrackEntity entity)
        {
            // Only asset-backed definitions have something to show. Anything else is logic-only.
            if (!(entity.Definition is TrackEntityDefinition definition) || definition.Prefab == null) return;

            EnsureCapacity(entity.Id);
            var view = PoolFor(definition.Prefab).Get();
            view.Place(entity.X, entity.Z, 0f);
            _views[entity.Id] = view;
            _prefabs[entity.Id] = definition.Prefab;
            _spinRates[entity.Id] = definition.SpinDegreesPerSecond;
        }

        private void OnRemoved(TrackEntity entity)
        {
            if (entity.Id >= _views.Length) return;

            var view = _views[entity.Id];
            if (ReferenceEquals(view, null)) return;

            _pools[_prefabs[entity.Id]].Release(view);
            _views[entity.Id] = null;
            _prefabs[entity.Id] = null;
        }

        private ComponentPool<TrackEntityView> PoolFor(TrackEntityView prefab)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new ComponentPool<TrackEntityView>(prefab, _entityRoot, PrewarmPerPrefab);
                _pools.Add(prefab, pool);
            }

            return pool;
        }

        private void EnsureCapacity(int id)
        {
            if (id < _views.Length) return;

            int size = Mathf.NextPowerOfTwo(id + 1);
            Array.Resize(ref _views, size);
            Array.Resize(ref _prefabs, size);
            Array.Resize(ref _spinRates, size);
        }
    }
}
