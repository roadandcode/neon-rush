using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Puts screens up and takes them down as the game changes phase, by applying the screen
    /// table. Screens never show or hide themselves, so what is on screen in any phase can be
    /// read off the table.
    /// </summary>
    internal sealed class ScreenSwitcher : IStartable, ITickable, IDisposable
    {
        private readonly ScreenTable _table;
        private readonly IGameFlow _flow;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public ScreenSwitcher(ScreenTable table, IGameFlow flow, ISubscriber subscriber)
        {
            _table = Guard.NotNull(table, nameof(table));
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        // The phase may already have moved on by the time the UI starts, so catch up before listening.
        public void Start()
        {
            _table.Apply(_flow.Phase);
            _subscription = _subscriber.Subscribe<GamePhaseChanged>(OnPhaseChanged);
        }

        // Screens that are held back count down in real time; the simulation is stopped when they are.
        public void Tick() => _table.Advance(Time.deltaTime);

        public void Dispose() => _subscription?.Dispose();

        private void OnPhaseChanged(GamePhaseChanged message) => _table.Apply(message.Current);
    }
}
