using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Presentation
{
    /// <summary>Turns motor state into a pose for the view, once per tick and once when a run resets.</summary>
    internal sealed class PlayerPresenter : ISimulationSystem, IDisposable
    {
        private readonly PlayerMotor _motor;
        private readonly PlayerView _view;
        private readonly PlayerTuning _tuning;
        private readonly IDisposable _subscription;

        public PlayerPresenter(PlayerMotor motor, PlayerView view, PlayerTuning tuning, ISubscriber subscriber)
        {
            _motor = Guard.NotNull(motor, nameof(motor));
            _view = Guard.NotNull(view, nameof(view));
            _tuning = Guard.NotNull(tuning, nameof(tuning));
            _subscription = Guard.NotNull(subscriber, nameof(subscriber)).Subscribe<RunStarted>(OnRunStarted);

            Render();
        }

        public void Tick(float deltaTime) => Render();

        public void Dispose() => _subscription.Dispose();

        private void OnRunStarted(RunStarted message) => Render();

        private void Render()
        {
            float lean = Mathf.Clamp((_motor.TargetX - _motor.X) * _tuning.LeanPerMetre, -_tuning.MaxLean, _tuning.MaxLean);
            float stature = _motor.BodyHeight / _tuning.StandingHeight;
            _view.Render(new PlayerPose(_motor.X, _motor.Height, stature, lean));
        }
    }
}
