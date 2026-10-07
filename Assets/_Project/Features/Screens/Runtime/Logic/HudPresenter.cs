using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Scoring;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// The in-run display: score, multiplier and the pause button. The score changes most frames,
    /// so nothing on this path may allocate.
    /// </summary>
    internal sealed class HudPresenter : IStartable, IDisposable
    {
        private const int RestingMultiplier = 1;

        private readonly IHudView _view;
        private readonly IGameFlow _flow;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public HudPresenter(IHudView view, IGameFlow flow, ISubscriber subscriber)
        {
            _view = Guard.NotNull(view, nameof(view));
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            Show(0, RestingMultiplier);
            _view.PausePressed += OnPausePressed;
            _subscription = _subscriber.Subscribe<ScoreChanged>(OnScoreChanged);
        }

        public void Dispose()
        {
            _view.PausePressed -= OnPausePressed;
            _subscription?.Dispose();
        }

        private void OnPausePressed() => _flow.Pause();

        private void OnScoreChanged(ScoreChanged message) => Show(message.Score, message.Multiplier);

        private void Show(int score, int multiplier)
        {
            _view.SetScore(score);
            _view.SetMultiplier(multiplier, multiplier > RestingMultiplier);
        }
    }
}
