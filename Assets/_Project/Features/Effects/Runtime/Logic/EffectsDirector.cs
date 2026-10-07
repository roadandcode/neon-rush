using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Track;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Effects.Logic
{
    /// <summary>
    /// Places effects in the world when the track reports a contact. Both kinds of contact happen
    /// at the runner, so that is where the bursts go: the track's messages don't need to carry a
    /// position for this.
    /// </summary>
    internal sealed class EffectsDirector : IStartable, IDisposable
    {
        private readonly IEffectsView _view;
        private readonly IRunnerBody _runner;
        private readonly ISubscriber _subscriber;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public EffectsDirector(IEffectsView view, IRunnerBody runner, ISubscriber subscriber)
        {
            _view = Guard.NotNull(view, nameof(view));
            _runner = Guard.NotNull(runner, nameof(runner));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            _subscriber.Subscribe<HazardHit>(OnHazardHit).AddTo(_subscriptions);
            _subscriber.Subscribe<PickupCollected>(OnPickupCollected).AddTo(_subscriptions);
        }

        public void Dispose() => _subscriptions.Dispose();

        private void OnHazardHit(HazardHit message) => _view.PlayImpact(_runner.Bounds.center);

        private void OnPickupCollected(PickupCollected message) => _view.PlayPickup(_runner.Bounds.center);
    }
}
