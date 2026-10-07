using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>
    /// Counts the intro down and then lets the run begin. The simulation is not running yet, so
    /// this keeps its own time from the player loop.
    /// </summary>
    internal sealed class IntroClock : IStartable, ITickable, IDisposable
    {
        private readonly GameFlow _flow;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;
        private float _remaining;
        private bool _counting;

        public IntroClock(GameFlow flow, ISubscriber subscriber)
        {
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start() => _subscription = _subscriber.Subscribe<RunIntroStarted>(OnIntroStarted);

        public void Tick() => Advance(Time.deltaTime);

        public void Dispose() => _subscription?.Dispose();

        internal void Advance(float deltaTime)
        {
            if (!_counting) return;

            _remaining -= deltaTime;
            if (_remaining > 0f) return;

            _counting = false;
            _flow.FinishIntro();
        }

        private void OnIntroStarted(RunIntroStarted message)
        {
            _remaining = message.Duration;
            _counting = true;
        }
    }
}
