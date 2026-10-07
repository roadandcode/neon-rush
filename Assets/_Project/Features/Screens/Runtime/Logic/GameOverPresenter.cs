using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Scoring;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>The end-of-run screen: the result, and a choice between going again and the title screen.</summary>
    internal sealed class GameOverPresenter : IStartable, IDisposable
    {
        private readonly IGameOverView _view;
        private readonly IGameFlow _flow;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public GameOverPresenter(IGameOverView view, IGameFlow flow, ISubscriber subscriber)
        {
            _view = Guard.NotNull(view, nameof(view));
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            _view.RetryPressed += OnRetryPressed;
            _view.MenuPressed += OnMenuPressed;
            _subscription = _subscriber.Subscribe<RunScored>(OnRunScored);
        }

        public void Dispose()
        {
            _view.RetryPressed -= OnRetryPressed;
            _view.MenuPressed -= OnMenuPressed;
            _subscription?.Dispose();
        }

        private void OnRetryPressed() => _flow.StartRun();

        private void OnMenuPressed() => _flow.ReturnToMenu();

        // The result arrives just after the phase change that put this screen up, within the same frame.
        private void OnRunScored(RunScored message) => _view.ShowResult(message.Score, message.Best, message.IsNewBest);
    }
}
