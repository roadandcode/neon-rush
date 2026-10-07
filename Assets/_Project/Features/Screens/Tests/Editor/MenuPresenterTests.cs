using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Scoring;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class MenuPresenterTests
    {
        private FakeMenuView _view;
        private FakeFlow _flow;
        private FakeBestScore _best;
        private MessageBus _bus;
        private MenuPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeMenuView();
            _flow = new FakeFlow();
            _best = new FakeBestScore { Best = 420 };
            _bus = new MessageBus();
            _presenter = new MenuPresenter(_view, _flow, _best, _bus);
        }

        [Test]
        public void Start_ShowsTheSavedBest()
        {
            _presenter.Start();

            Assert.That(_view.Best, Is.EqualTo(420));
        }

        [Test]
        public void PressingPlay_StartsARun()
        {
            _presenter.Start();

            _view.PressPlay();

            Assert.That(_flow.RunsStarted, Is.EqualTo(1));
        }

        [Test]
        public void ARunResult_UpdatesTheBest()
        {
            _presenter.Start();

            _bus.Publish(new RunScored(score: 900, best: 900, isNewBest: true));

            Assert.That(_view.Best, Is.EqualTo(900));
        }

        [Test]
        public void AfterDispose_ThePresenterHasLetGoOfTheView()
        {
            _presenter.Start();
            _presenter.Dispose();

            _view.PressPlay();
            _bus.Publish(new RunScored(score: 900, best: 900, isNewBest: true));

            Assert.That(_flow.RunsStarted, Is.Zero);
            Assert.That(_view.Best, Is.EqualTo(420));
        }
    }
}
