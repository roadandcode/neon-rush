using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Scoring;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>The title screen: shows the best score so far and starts a run.</summary>
    internal sealed class MenuPresenter : IStartable, IDisposable
    {
        private readonly IMenuView _view;
        private readonly IGameFlow _flow;
        private readonly IBestScore _best;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public MenuPresenter(IMenuView view, IGameFlow flow, IBestScore best, ISubscriber subscriber)
        {
            _view = Guard.NotNull(view, nameof(view));
            _flow = Guard.NotNull(flow, nameof(flow));
            _best = Guard.NotNull(best, nameof(best));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        // The saved best is read once. After that every change to it arrives with a run's result.
        public void Start()
        {
            _view.SetBest(_best.Best);
            _view.PlayPressed += OnPlayPressed;
            _subscription = _subscriber.Subscribe<RunScored>(OnRunScored);
        }

        public void Dispose()
        {
            _view.PlayPressed -= OnPlayPressed;
            _subscription?.Dispose();
        }

        private void OnPlayPressed() => _flow.StartRun();

        private void OnRunScored(RunScored message) => _view.SetBest(message.Best);
    }
}
