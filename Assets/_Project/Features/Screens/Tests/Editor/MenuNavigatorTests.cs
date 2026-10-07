using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Screens;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class MenuNavigatorTests
    {
        private FakeMenuInput _input;
        private FakeButtonPressView _pointer;
        private FakeFlow _flow;
        private MessageBus _bus;
        private FakeMenuView _menu;
        private FakePauseView _pause;
        private FakeGameOverView _gameOver;
        private MenuNavigator _navigator;
        private int _announced;

        [SetUp]
        public void SetUp()
        {
            _input = new FakeMenuInput();
            _pointer = new FakeButtonPressView();
            _flow = new FakeFlow { Phase = GamePhase.Menu };
            _bus = new MessageBus();
            _menu = new FakeMenuView { ControlCount = 2 };
            _pause = new FakePauseView { ControlCount = 3 };
            _gameOver = new FakeGameOverView { ControlCount = 2 };
            _announced = 0;
            _bus.Subscribe<ButtonPressed>(_ => _announced++);

            _navigator = new MenuNavigator(_input, _pointer, _flow, _bus, _bus, _menu, _pause, _gameOver);
            _navigator.Start();
        }

        private void Enter(GamePhase phase)
        {
            GamePhase previous = _flow.Phase;
            _flow.Phase = phase;
            _bus.Publish(new GamePhaseChanged(previous, phase));
        }

        [Test]
        public void SubmitAlone_PressesTheScreensFirstControl()
        {
            _input.Submit();

            Assert.That(_menu.Pressed, Is.EqualTo(new[] { 0 }));
            Assert.That(_announced, Is.EqualTo(1), "A key press on a button is announced like any other press.");
        }

        [Test]
        public void NothingIsHighlighted_UntilThePlayerUsesKeys()
        {
            Assert.That(_menu.Focus, Is.EqualTo(ControlFocus.None));
        }

        [Test]
        public void TheFirstMove_ShowsWhereThePlayerIs_WithoutMovingThem()
        {
            _input.Next();

            Assert.That(_menu.Focus, Is.Zero);
        }

        [Test]
        public void FurtherMoves_StepThroughTheControls_AndWrapAtEitherEnd()
        {
            Enter(GamePhase.Run);
            Enter(GamePhase.Paused);
            var seen = new List<int>();

            _input.Next();
            seen.Add(_pause.Focus);
            _input.Next();
            seen.Add(_pause.Focus);
            _input.Next();
            seen.Add(_pause.Focus);
            _input.Next();
            seen.Add(_pause.Focus);
            _input.Previous();
            seen.Add(_pause.Focus);

            Assert.That(seen, Is.EqualTo(new[] { 0, 1, 2, 0, 2 }));
        }

        [Test]
        public void Submit_PressesTheControlThePlayerMovedTo()
        {
            Enter(GamePhase.Run);
            Enter(GamePhase.GameOver);
            _input.Next();
            _input.Next();

            _input.Submit();

            Assert.That(_gameOver.Pressed, Is.EqualTo(new[] { 1 }));
            Assert.That(_menu.Pressed, Is.Empty);
        }

        [Test]
        public void EachScreen_StartsOnItsFirstControl_AndTheOldScreenLosesItsHighlight()
        {
            _input.Next();
            _input.Next();
            Assert.That(_menu.Focus, Is.EqualTo(1));

            Enter(GamePhase.Run);
            Enter(GamePhase.GameOver);

            Assert.That(_menu.Focus, Is.EqualTo(ControlFocus.None));
            Assert.That(_gameOver.Focus, Is.Zero, "Once keys are in use the highlight is there on every screen.");
        }

        [TestCase(GamePhase.Menu, true)]
        [TestCase(GamePhase.Intro, false)]
        [TestCase(GamePhase.Run, false)]
        [TestCase(GamePhase.Paused, true)]
        [TestCase(GamePhase.GameOver, true)]
        public void MenuKeys_AreOnlyHeardWhileAScreenWithControlsIsUp(GamePhase phase, bool listening)
        {
            Enter(phase);

            Assert.That(_input.Enabled, Is.EqualTo(listening));
        }

        [Test]
        public void DuringARun_SubmitAndMoveDoNothing()
        {
            Enter(GamePhase.Run);

            _input.Next();
            _input.Submit();

            Assert.That(_menu.Pressed, Is.Empty);
            Assert.That(_pause.Pressed, Is.Empty);
            Assert.That(_gameOver.Pressed, Is.Empty);
            Assert.That(_announced, Is.Zero);
        }

        [Test]
        public void APointerPress_TakesTheHighlightAway_AndTheNextKeyBringsItBackWhereItWas()
        {
            _input.Next();
            _input.Next();
            Assert.That(_menu.Focus, Is.EqualTo(1));

            _pointer.Press();
            Assert.That(_menu.Focus, Is.EqualTo(ControlFocus.None));

            _input.Next();
            Assert.That(_menu.Focus, Is.EqualTo(1));
        }

        [Test]
        public void AScreenWithNoControls_IsLeftAlone()
        {
            _menu.ControlCount = 0;

            _input.Next();
            _input.Submit();

            Assert.That(_menu.Pressed, Is.Empty);
            Assert.That(_menu.Focus, Is.EqualTo(ControlFocus.None));
        }

        [Test]
        public void AfterDispose_KeysDoNothing()
        {
            _navigator.Dispose();

            _input.Submit();
            Enter(GamePhase.Paused);
            _input.Submit();

            Assert.That(_menu.Pressed, Is.Empty);
            Assert.That(_pause.Pressed, Is.Empty);
        }
    }
}
