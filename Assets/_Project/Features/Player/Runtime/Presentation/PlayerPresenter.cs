using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Presentation
{
    /// <summary>
    /// Turns motor state into a pose for the view: once per tick, and whenever the runner changes
    /// while the simulation is not ticking (a reset, or the hit that ends the run).
    /// </summary>
    internal sealed class PlayerPresenter : ISimulationSystem, IDisposable
    {
        private readonly PlayerMotor _motor;
        private readonly PlayerView _view;
        private readonly PlayerTuning _tuning;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public PlayerPresenter(PlayerMotor motor, PlayerView view, PlayerTuning tuning, ISubscriber subscriber)
        {
            _motor = Guard.NotNull(motor, nameof(motor));
            _view = Guard.NotNull(view, nameof(view));
            _tuning = Guard.NotNull(tuning, nameof(tuning));
            Guard.NotNull(subscriber, nameof(subscriber));

            subscriber.Subscribe<RunStarted>(OnRunStarted).AddTo(_subscriptions);
            subscriber.Subscribe<RunEnded>(OnRunEnded).AddTo(_subscriptions);
            subscriber.Subscribe<StageCleared>(OnStageCleared).AddTo(_subscriptions);

            Render();
        }

        public void Tick(float deltaTime) => Render();

        public void Dispose() => _subscriptions.Dispose();

        private void OnRunStarted(RunStarted message) => Render();

        // The simulation stops with the hit, so a tick would never draw the knocked-down pose.
        private void OnRunEnded(RunEnded message) => Render();

        private void OnStageCleared(StageCleared message) => Render();

        private void Render()
        {
            float lean = Mathf.Clamp((_motor.TargetX - _motor.X) * _tuning.LeanPerMetre, -_tuning.MaxLean, _tuning.MaxLean);
            float stature = _motor.BodyHeight / _tuning.StandingHeight;
            float tilt = _motor.Mode == PlayerMode.Down ? _tuning.KnockDownTilt : 0f;
            _view.Render(new PlayerPose(_motor.X, _motor.Height, stature, lean, tilt));
        }
    }
}
