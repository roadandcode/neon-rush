using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Scoring;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class HudPresenterTests
    {
        private FakeHudView _view;
        private FakeFlow _flow;
        private MessageBus _bus;
        private HudPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeHudView();
            _flow = new FakeFlow();
            _bus = new MessageBus();
            _presenter = new HudPresenter(_view, _flow, _bus);
        }

        [Test]
        public void Start_ShowsAnEmptyScore()
        {
            _presenter.Start();

            Assert.That(_view.Score, Is.Zero);
            Assert.That(_view.Multiplier, Is.EqualTo(1));
            Assert.That(_view.ComboActive, Is.False);
        }

        [Test]
        public void AScoreChange_IsShown()
        {
            _presenter.Start();

            _bus.Publish(new ScoreChanged(score: 1234, multiplier: 1));

            Assert.That(_view.Score, Is.EqualTo(1234));
            Assert.That(_view.ComboActive, Is.False);
        }

        [Test]
        public void AMultiplierAboveOne_IsACombo_UntilItDropsBack()
        {
            _presenter.Start();

            _bus.Publish(new ScoreChanged(score: 50, multiplier: 3));
            Assert.That(_view.Multiplier, Is.EqualTo(3));
            Assert.That(_view.ComboActive, Is.True);

            _bus.Publish(new ScoreChanged(score: 60, multiplier: 1));
            Assert.That(_view.Multiplier, Is.EqualTo(1));
            Assert.That(_view.ComboActive, Is.False);
        }

        [Test]
        public void PressingPause_PausesTheGame()
        {
            _presenter.Start();

            _view.PressPause();

            Assert.That(_flow.Pauses, Is.EqualTo(1));
        }

        [Test]
        public void AfterDispose_ThePresenterHasLetGoOfTheView()
        {
            _presenter.Start();
            _presenter.Dispose();

            _view.PressPause();
            _bus.Publish(new ScoreChanged(score: 77, multiplier: 2));

            Assert.That(_flow.Pauses, Is.Zero);
            Assert.That(_view.Score, Is.Zero);
        }

        [Test]
        public void ShowingScoreChanges_AllocatesNothing()
        {
            _presenter.Start();
            _bus.Publish(new ScoreChanged(score: 1, multiplier: 1));

            Assert.That(() =>
            {
                for (int i = 0; i < 100; i++) _bus.Publish(new ScoreChanged(score: i * 37, multiplier: 1 + (i % 4)));
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
