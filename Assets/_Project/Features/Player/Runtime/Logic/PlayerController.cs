using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Track;

namespace RoadAndCode.NeonRush.Player.Logic
{
    /// <summary>
    /// Feeds player actions into the motor each tick and decides what a hazard hit means
    /// for the runner. Today a hit ends the run; a shield or extra life would be a change here only.
    /// </summary>
    internal sealed class PlayerController : ISimulationSystem, IDisposable
    {
        private readonly PlayerMotor _motor;
        private readonly IPlayerActionSource _actions;
        private readonly PlayerTuning _tuning;
        private readonly IGameFlow _flow;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private PlayerAction _buffered;
        private float _bufferedFor;
        private bool _hasBuffered;

        public PlayerController(
            PlayerMotor motor,
            IPlayerActionSource actions,
            PlayerTuning tuning,
            IGameFlow flow,
            ISubscriber subscriber)
        {
            _motor = Guard.NotNull(motor, nameof(motor));
            _actions = Guard.NotNull(actions, nameof(actions));
            _tuning = Guard.NotNull(tuning, nameof(tuning));
            _flow = Guard.NotNull(flow, nameof(flow));
            Guard.NotNull(subscriber, nameof(subscriber));

            subscriber.Subscribe<RunStarted>(OnRunStarted).AddTo(_subscriptions);
            subscriber.Subscribe<GamePhaseChanged>(OnPhaseChanged).AddTo(_subscriptions);
            subscriber.Subscribe<HazardHit>(OnHazardHit).AddTo(_subscriptions);

            _actions.SetEnabled(_flow.Phase == GamePhase.Run);
        }

        public void Tick(float deltaTime)
        {
            while (_actions.TryDequeue(out var action))
            {
                if (!_motor.Apply(action)) Buffer(action);
            }

            RetryBuffered(deltaTime);
            _motor.Tick(deltaTime);
        }

        public void Dispose() => _subscriptions.Dispose();

        // Only jump and slide are worth remembering: a lane change that fails has hit the edge of the track.
        private void Buffer(PlayerAction action)
        {
            if (action != PlayerAction.Jump && action != PlayerAction.Slide) return;

            _buffered = action;
            _bufferedFor = 0f;
            _hasBuffered = true;
        }

        private void RetryBuffered(float deltaTime)
        {
            if (!_hasBuffered) return;

            if (_motor.Apply(_buffered))
            {
                _hasBuffered = false;
                return;
            }

            _bufferedFor += deltaTime;
            if (_bufferedFor > _tuning.InputBufferTime) _hasBuffered = false;
        }

        private void OnRunStarted(RunStarted message)
        {
            _hasBuffered = false;
            _motor.Reset();
        }

        // Input only counts during a run. Disabling the source also drops anything pressed in a menu.
        private void OnPhaseChanged(GamePhaseChanged message)
        {
            _actions.SetEnabled(message.Current == GamePhase.Run);
        }

        private void OnHazardHit(HazardHit message)
        {
            _motor.KnockDown();
            _flow.FailRun();
        }
    }
}
