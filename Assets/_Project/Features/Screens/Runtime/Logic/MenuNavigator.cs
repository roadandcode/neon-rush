using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Screens;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Makes every screen usable from keys or a gamepad. It tracks which control is current on
    /// the screen that belongs to the phase, moves that with "previous" and "next", and presses
    /// it on "submit". The first control of each screen is its default, so submit alone starts a
    /// run from the menu and retries from game over.
    ///
    /// The highlight is only drawn once the player has used keys or a pad, and goes away again
    /// when they press something with a pointer: a touch player never sees a button lit up for
    /// no reason, and a mouse player is not left with a stale one.
    /// </summary>
    internal sealed class MenuNavigator : IStartable, IDisposable
    {
        private readonly IMenuInput _input;
        private readonly IButtonPressView _pointerPresses;
        private readonly IGameFlow _flow;
        private readonly ISubscriber _subscriber;
        private readonly IPublisher _publisher;
        private readonly IMenuView _menu;
        private readonly IPauseView _pause;
        private readonly IGameOverView _gameOver;
        private IDisposable _subscription;

        private IControlList _current;
        private int _index;
        private bool _highlighting;

        public MenuNavigator(
            IMenuInput input,
            IButtonPressView pointerPresses,
            IGameFlow flow,
            ISubscriber subscriber,
            IPublisher publisher,
            IMenuView menu,
            IPauseView pause,
            IGameOverView gameOver)
        {
            _input = Guard.NotNull(input, nameof(input));
            _pointerPresses = Guard.NotNull(pointerPresses, nameof(pointerPresses));
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
            _publisher = Guard.NotNull(publisher, nameof(publisher));
            _menu = Guard.NotNull(menu, nameof(menu));
            _pause = Guard.NotNull(pause, nameof(pause));
            _gameOver = Guard.NotNull(gameOver, nameof(gameOver));
        }

        public void Start()
        {
            _input.Moved += OnMoved;
            _input.Submitted += OnSubmitted;
            _pointerPresses.ButtonPressed += OnPointerPress;
            _subscription = _subscriber.Subscribe<GamePhaseChanged>(OnPhaseChanged);

            Enter(_flow.Phase);
        }

        public void Dispose()
        {
            _input.Moved -= OnMoved;
            _input.Submitted -= OnSubmitted;
            _pointerPresses.ButtonPressed -= OnPointerPress;
            _subscription?.Dispose();
        }

        private void OnPhaseChanged(GamePhaseChanged message) => Enter(message.Current);

        // During a run and its intro there is nothing to navigate, and the same keys are the runner's.
        private void Enter(GamePhase phase)
        {
            _current?.SetFocus(ControlFocus.None);

            _current = ControlsFor(phase);
            _index = 0;
            _input.SetEnabled(_current != null);
            ShowHighlight();
        }

        private IControlList ControlsFor(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Menu: return _menu;
                case GamePhase.Paused: return _pause;
                case GamePhase.GameOver: return _gameOver;
                default: return null;
            }
        }

        private void OnMoved(int direction)
        {
            if (_current == null || _current.ControlCount == 0) return;

            // The first key press only reveals where the player is; it does not also move them.
            if (_highlighting) _index = Wrap(_index + direction, _current.ControlCount);

            _highlighting = true;
            ShowHighlight();
        }

        private void OnSubmitted()
        {
            if (_current == null || _current.ControlCount == 0) return;

            _highlighting = true;
            ShowHighlight();
            _publisher.Publish(new ButtonPressed());

            // Pressing may change the phase, which replaces _current before this call returns.
            _current.Press(_index);
        }

        private void OnPointerPress()
        {
            _highlighting = false;
            ShowHighlight();
        }

        private void ShowHighlight() => _current?.SetFocus(_highlighting ? _index : ControlFocus.None);

        private static int Wrap(int index, int count) => ((index % count) + count) % count;
    }
}
