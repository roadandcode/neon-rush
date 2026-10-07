using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.App.Tests
{
    public sealed class SimulationDriverTests
    {
        private sealed class Counter : ISimulationSystem
        {
            public int Ticks { get; private set; }

            public void Tick(float deltaTime) => Ticks++;
        }

        private sealed class StubFlow : IGameFlow
        {
            public GamePhase Phase { get; set; }

            public bool StartRun() => false;

            public bool Pause() => false;

            public bool Resume() => false;

            public bool FailRun() => false;

            public bool ReturnToMenu() => false;
        }

        private MessageBus _bus;
        private Counter _counter;
        private SimulationLoop _simulation;
        private StubFlow _flow;
        private SimulationDriver _driver;

        [SetUp]
        public void SetUp()
        {
            _bus = new MessageBus();
            _counter = new Counter();
            _simulation = new SimulationLoop(new ISimulationSystem[] { _counter });
            _flow = new StubFlow { Phase = GamePhase.Menu };
            _driver = new SimulationDriver(_simulation, _flow, _bus);
        }

        [Test]
        public void Start_OutsideARun_LeavesTheSimulationStopped()
        {
            _driver.Start();
            _driver.Tick();

            Assert.That(_simulation.IsRunning, Is.False);
            Assert.That(_counter.Ticks, Is.Zero);
        }

        [Test]
        public void Start_DuringARun_PicksUpWhereTheFlowIs()
        {
            _flow.Phase = GamePhase.Run;

            _driver.Start();

            Assert.That(_simulation.IsRunning, Is.True);
        }

        [Test]
        public void SimulationAdvances_OnlyWhileThePhaseIsRun()
        {
            _driver.Start();

            _bus.Publish(new GamePhaseChanged(GamePhase.Menu, GamePhase.Run));
            _driver.Tick();
            Assert.That(_counter.Ticks, Is.EqualTo(1));

            _bus.Publish(new GamePhaseChanged(GamePhase.Run, GamePhase.Paused));
            _driver.Tick();
            Assert.That(_counter.Ticks, Is.EqualTo(1));

            _bus.Publish(new GamePhaseChanged(GamePhase.Paused, GamePhase.Run));
            _driver.Tick();
            Assert.That(_counter.Ticks, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_StopsTheSimulation_AndIgnoresLaterPhaseChanges()
        {
            _driver.Start();
            _bus.Publish(new GamePhaseChanged(GamePhase.Menu, GamePhase.Run));

            _driver.Dispose();
            _bus.Publish(new GamePhaseChanged(GamePhase.GameOver, GamePhase.Run));

            Assert.That(_simulation.IsRunning, Is.False);
        }
    }
}
