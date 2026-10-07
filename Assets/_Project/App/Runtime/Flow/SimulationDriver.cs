using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>
    /// Bridges Unity's player loop to the simulation, and lets it advance only during a run.
    /// Pausing the game is this switch; no gameplay system has to know what a pause is.
    /// </summary>
    internal sealed class SimulationDriver : IStartable, ITickable, IDisposable
    {
        // A browser tab coming back from the background can report seconds of delta time.
        // Simulating that in one step would carry the runner straight through whatever was ahead.
        private const float MaxStep = 1f / 20f;

        private readonly SimulationLoop _simulation;
        private readonly IGameFlow _flow;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public SimulationDriver(SimulationLoop simulation, IGameFlow flow, ISubscriber subscriber)
        {
            _simulation = Guard.NotNull(simulation, nameof(simulation));
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            _subscription = _subscriber.Subscribe<GamePhaseChanged>(OnPhaseChanged);
            _simulation.IsRunning = _flow.Phase == GamePhase.Run;
        }

        public void Tick()
        {
            _simulation.Tick(Mathf.Min(Time.deltaTime, MaxStep));
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _simulation.IsRunning = false;
        }

        private void OnPhaseChanged(GamePhaseChanged message)
        {
            _simulation.IsRunning = message.Current == GamePhase.Run;
        }
    }
}
