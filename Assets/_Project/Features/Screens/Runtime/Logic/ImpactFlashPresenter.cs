using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Track;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>Flashes the screen when the runner hits something.</summary>
    internal sealed class ImpactFlashPresenter : IStartable, IDisposable
    {
        private readonly IImpactFlashView _view;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public ImpactFlashPresenter(IImpactFlashView view, ISubscriber subscriber)
        {
            _view = Guard.NotNull(view, nameof(view));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start() => _subscription = _subscriber.Subscribe<HazardHit>(OnHazardHit);

        public void Dispose() => _subscription?.Dispose();

        private void OnHazardHit(HazardHit message) => _view.Flash();
    }
}
