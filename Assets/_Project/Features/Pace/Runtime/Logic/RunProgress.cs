using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;

namespace RoadAndCode.NeonRush.Pace.Logic
{
    /// <summary>
    /// The clock of a run. Ticks first, so every other system reads this frame's speed and distance.
    /// </summary>
    internal sealed class RunProgress : IRunProgress, ISimulationSystem, IDisposable
    {
        private readonly IDifficultyCurve _curve;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public RunProgress(IDifficultyCurve curve, ISubscriber subscriber)
        {
            _curve = Guard.NotNull(curve, nameof(curve));
            Guard.NotNull(subscriber, nameof(subscriber));

            subscriber.Subscribe<RunStarted>(OnRunStarted).AddTo(_subscriptions);
            subscriber.Subscribe<StageCleared>(OnStageCleared).AddTo(_subscriptions);
            Reset();
        }

        public float Elapsed { get; private set; }

        public float Distance { get; private set; }

        public float Speed { get; private set; }

        public int Tier { get; private set; }

        public void Tick(float deltaTime)
        {
            Elapsed += deltaTime;
            Speed = _curve.SpeedAt(Elapsed);
            Tier = _curve.TierAt(Elapsed);
            Distance += Speed * deltaTime;
        }

        public void Dispose() => _subscriptions.Dispose();

        private void OnRunStarted(RunStarted message) => Reset();

        private void OnStageCleared(StageCleared message) => Reset();

        private void Reset()
        {
            Elapsed = 0f;
            Distance = 0f;
            Speed = _curve.SpeedAt(0f);
            Tier = _curve.TierAt(0f);
        }
    }
}
