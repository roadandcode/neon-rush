using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Randomness;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Track.Data;

namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>
    /// One tick of the track: bring everything closer, recycle what has gone past, let new rows
    /// in, then check what the runner is touching. The runner never moves forward; the track
    /// comes to it, which keeps coordinates small however long the run lasts.
    /// </summary>
    internal sealed class TrackSimulation : ISimulationSystem, IDisposable
    {
        private readonly TrackField _field;
        private readonly TrackSpawner _spawner;
        private readonly IRunProgress _progress;
        private readonly IRunnerBody _runner;
        private readonly IPublisher _publisher;
        private readonly TrackSettings _settings;
        private readonly SeededRandom _random;
        private readonly IDisposable _subscription;

        public TrackSimulation(
            TrackField field,
            TrackSpawner spawner,
            IRunProgress progress,
            IRunnerBody runner,
            IPublisher publisher,
            ISubscriber subscriber,
            TrackSettings settings,
            SeededRandom random)
        {
            _field = Guard.NotNull(field, nameof(field));
            _spawner = Guard.NotNull(spawner, nameof(spawner));
            _progress = Guard.NotNull(progress, nameof(progress));
            _runner = Guard.NotNull(runner, nameof(runner));
            _publisher = Guard.NotNull(publisher, nameof(publisher));
            _settings = Guard.NotNull(settings, nameof(settings));
            _random = Guard.NotNull(random, nameof(random));
            _subscription = Guard.NotNull(subscriber, nameof(subscriber)).Subscribe<RunStarted>(OnRunStarted);
        }

        public void Tick(float deltaTime)
        {
            float distance = _progress.Speed * deltaTime;

            AdvanceAndRecycle(distance);
            _spawner.Advance(distance, _progress.Speed, _progress.Tier);
            ResolveContacts();
        }

        public void Dispose() => _subscription.Dispose();

        // The same seed lays out the same track, which is what makes a run reproducible.
        private void OnRunStarted(RunStarted message)
        {
            _random.Reseed(message.Seed);
            _field.Clear();
            _spawner.Reset();
        }

        private void AdvanceAndRecycle(float distance)
        {
            var entities = _field.Entities;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                entity.Advance(distance);
                if (entity.Z < -_settings.DespawnDistance) _field.RemoveAt(i);
            }
        }

        private void ResolveContacts()
        {
            var body = _runner.Bounds;
            var entities = _field.Entities;

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                if (entity.Touched || !entity.Bounds.Intersects(body)) continue;

                entity.MarkTouched();
                if (entity.Definition.OnTouched(_publisher)) _field.RemoveAt(i);
            }
        }
    }
}
