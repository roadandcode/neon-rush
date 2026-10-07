using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Scoring;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class PausePresenterTests
    {
        private FakePauseView _view;
        private FakeFlow _flow;
        private PausePresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakePauseView();
            _flow = new FakeFlow();
            _presenter = new PausePresenter(_view, _flow);
            _presenter.Start();
        }

        [Test]
        public void PressingResume_ResumesTheRun()
        {
            _view.PressResume();

            Assert.That(_flow.Resumes, Is.EqualTo(1));
            Assert.That(_flow.ReturnsToMenu, Is.Zero);
        }

        [Test]
        public void PressingQuit_GoesBackToTheMenu()
        {
            _view.PressQuit();

            Assert.That(_flow.ReturnsToMenu, Is.EqualTo(1));
            Assert.That(_flow.Resumes, Is.Zero);
        }

        [Test]
        public void AfterDispose_TheButtonsDoNothing()
        {
            _presenter.Dispose();

            _view.PressResume();
            _view.PressQuit();

            Assert.That(_flow.Resumes, Is.Zero);
            Assert.That(_flow.ReturnsToMenu, Is.Zero);
        }
    }

    public sealed class GameOverPresenterTests
    {
        private FakeGameOverView _view;
        private FakeFlow _flow;
        private MessageBus _bus;
        private GameOverPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeGameOverView();
            _flow = new FakeFlow();
            _bus = new MessageBus();
            _presenter = new GameOverPresenter(_view, _flow, _bus);
            _presenter.Start();
        }

        [Test]
        public void ARunResult_IsShown()
        {
            _bus.Publish(new RunScored(score: 310, best: 800, isNewBest: false));

            Assert.That(_view.Result, Is.EqualTo((310, 800, false)));
        }

        [Test]
        public void ANewBest_IsPassedOnAsOne()
        {
            _bus.Publish(new RunScored(score: 950, best: 950, isNewBest: true));

            Assert.That(_view.Result, Is.EqualTo((950, 950, true)));
        }

        [Test]
        public void PressingRetry_StartsARun()
        {
            _view.PressRetry();

            Assert.That(_flow.RunsStarted, Is.EqualTo(1));
        }

        [Test]
        public void PressingMenu_GoesBackToTheMenu()
        {
            _view.PressMenu();

            Assert.That(_flow.ReturnsToMenu, Is.EqualTo(1));
        }

        [Test]
        public void AfterDispose_ThePresenterHasLetGoOfTheView()
        {
            _presenter.Dispose();

            _view.PressRetry();
            _bus.Publish(new RunScored(score: 5, best: 5, isNewBest: true));

            Assert.That(_flow.RunsStarted, Is.Zero);
            Assert.That(_view.Result, Is.Null);
        }
    }
}
