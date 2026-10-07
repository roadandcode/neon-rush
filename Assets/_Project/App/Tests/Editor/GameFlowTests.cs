using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.App.Tests
{
    public sealed class GameFlowTests
    {
        private sealed class FixedSeeds : IRunSeedSource
        {
            private int _next = 100;

            public int NextSeed() => _next++;
        }

        private MessageBus _bus;
        private GameFlow _flow;
        private List<string> _events;

        [SetUp]
        public void SetUp()
        {
            _bus = new MessageBus();
            _flow = new GameFlow(_bus, new FixedSeeds());
            _events = new List<string>();

            _bus.Subscribe<GamePhaseChanged>(m => _events.Add($"{m.Previous}>{m.Current}"));
            _bus.Subscribe<RunStarted>(m => _events.Add($"started:{m.Seed}"));
            _bus.Subscribe<RunEnded>(m => _events.Add($"ended:{m.Reason}"));
        }

        private void BootToMenu()
        {
            _flow.Start();
            _flow.FinishBoot();
            _events.Clear();
        }

        [Test]
        public void Boot_EndsInMenu()
        {
            _flow.Start();
            _flow.FinishBoot();

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Menu));
            Assert.That(_events, Is.EqualTo(new[] { "Boot>Menu" }));
        }

        [Test]
        public void StartRun_FromMenu_AnnouncesTheRunBeforeEnteringIt()
        {
            BootToMenu();

            bool started = _flow.StartRun();

            Assert.That(started, Is.True);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Run));
            Assert.That(_events, Is.EqualTo(new[] { "started:100", "Menu>Run" }));
        }

        [Test]
        public void StartRun_BeforeBootFinishes_IsRejected()
        {
            _flow.Start();

            Assert.That(_flow.StartRun(), Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void PauseThenResume_ContinuesTheSameRun()
        {
            BootToMenu();
            _flow.StartRun();
            _events.Clear();

            Assert.That(_flow.Pause(), Is.True);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Paused));

            Assert.That(_flow.Resume(), Is.True);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Run));
            Assert.That(_events, Is.EqualTo(new[] { "Run>Paused", "Paused>Run" }));
        }

        [Test]
        public void StartRun_WhilePaused_IsRejected()
        {
            BootToMenu();
            _flow.StartRun();
            _flow.Pause();
            _events.Clear();

            Assert.That(_flow.StartRun(), Is.False);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Paused));
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void PauseAndResume_OutsideARun_AreRejected()
        {
            BootToMenu();

            Assert.That(_flow.Pause(), Is.False);
            Assert.That(_flow.Resume(), Is.False);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Menu));
        }

        [Test]
        public void FailRun_LeavesTheRunPhase_ThenReportsTheCrash()
        {
            BootToMenu();
            _flow.StartRun();
            _events.Clear();

            Assert.That(_flow.FailRun(), Is.True);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(_events, Is.EqualTo(new[] { "Run>GameOver", "ended:Crashed" }));
        }

        [Test]
        public void FailRun_WhenNotRunning_IsRejected()
        {
            BootToMenu();

            Assert.That(_flow.FailRun(), Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void StartRun_FromGameOver_BeginsANewRunWithANewSeed()
        {
            BootToMenu();
            _flow.StartRun();
            _flow.FailRun();
            _events.Clear();

            Assert.That(_flow.StartRun(), Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "started:101", "GameOver>Run" }));
        }

        [Test]
        public void ReturnToMenu_FromPause_AbandonsTheRun()
        {
            BootToMenu();
            _flow.StartRun();
            _flow.Pause();
            _events.Clear();

            Assert.That(_flow.ReturnToMenu(), Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "Paused>Menu", "ended:Abandoned" }));
        }

        [Test]
        public void ReturnToMenu_FromGameOver_DoesNotEndTheRunTwice()
        {
            BootToMenu();
            _flow.StartRun();
            _flow.FailRun();
            _events.Clear();

            Assert.That(_flow.ReturnToMenu(), Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "GameOver>Menu" }));
        }

        [Test]
        public void ReturnToMenu_MidRun_IsRejected()
        {
            BootToMenu();
            _flow.StartRun();

            Assert.That(_flow.ReturnToMenu(), Is.False);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Run));
        }
    }
}
