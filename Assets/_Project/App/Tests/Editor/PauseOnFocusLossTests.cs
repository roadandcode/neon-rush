using System;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.App.Tests
{
    public sealed class PauseOnFocusLossTests
    {
        private sealed class FakeFocus : IAppFocus
        {
            public event Action<bool> Changed;

            public bool HasFocus { get; private set; } = true;

            public void Set(bool focused)
            {
                HasFocus = focused;
                Changed?.Invoke(focused);
            }
        }

        private sealed class OneSeed : IRunSeedSource
        {
            public int NextSeed() => 1;
        }

        private FakeFocus _focus;
        private GameFlow _flow;
        private PauseOnFocusLoss _rule;

        [SetUp]
        public void SetUp()
        {
            _focus = new FakeFocus();
            _flow = new GameFlow(new MessageBus(), new OneSeed(), new FlowSettings(introSeconds: 0f));
            _flow.Start();
            _flow.FinishBoot();
            _rule = new PauseOnFocusLoss(_focus, _flow);
            _rule.Start();
        }

        [Test]
        public void LosingFocusDuringARun_PausesIt()
        {
            _flow.StartRun();

            _focus.Set(false);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Paused));
        }

        [Test]
        public void ComingBack_DoesNotResume()
        {
            _flow.StartRun();
            _focus.Set(false);

            _focus.Set(true);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Paused));
        }

        [Test]
        public void LosingFocusOnTheMenu_ChangesNothing()
        {
            _focus.Set(false);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Menu));
        }

        [Test]
        public void LosingFocusOnTheGameOverScreen_ChangesNothing()
        {
            _flow.StartRun();
            _flow.FailRun();

            _focus.Set(false);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.GameOver));
        }

        [Test]
        public void AfterDispose_FocusIsIgnored()
        {
            _flow.StartRun();
            _rule.Dispose();

            _focus.Set(false);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Run));
        }
    }
}
