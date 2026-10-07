using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Player.Tests
{
    public sealed class PlayerControllerTests
    {
        private sealed class ScriptedActions : IPlayerActionSource
        {
            private readonly Queue<PlayerAction> _queue = new Queue<PlayerAction>();

            public bool Enabled { get; private set; }

            public void Press(PlayerAction action) => _queue.Enqueue(action);

            public void SetEnabled(bool enabled)
            {
                Enabled = enabled;
                if (!enabled) _queue.Clear();
            }

            public bool TryDequeue(out PlayerAction action)
            {
                if (_queue.Count == 0)
                {
                    action = default;
                    return false;
                }

                action = _queue.Dequeue();
                return true;
            }
        }

        private sealed class StubFlow : IGameFlow
        {
            public GamePhase Phase { get; set; } = GamePhase.Run;

            public int FailedRuns { get; private set; }

            public bool StartRun() => false;

            public bool Pause() => false;

            public bool Resume() => false;

            public bool FailRun()
            {
                FailedRuns++;
                return true;
            }

            public bool ReturnToMenu() => false;
        }

        private const float Step = 1f / 120f;

        private PlayerTuning _tuning;
        private PlayerMotor _motor;
        private ScriptedActions _actions;
        private StubFlow _flow;
        private MessageBus _bus;
        private PlayerController _controller;

        [SetUp]
        public void SetUp()
        {
            _tuning = new PlayerTuning();
            _motor = new PlayerMotor(_tuning, new LaneGrid(3, 2.5f));
            _actions = new ScriptedActions();
            _flow = new StubFlow();
            _bus = new MessageBus();
            _controller = new PlayerController(_motor, _actions, _tuning, _flow, _bus, _bus);
        }

        private void Simulate(float seconds)
        {
            for (float t = 0f; t < seconds; t += Step) _controller.Tick(Step);
        }

        [Test]
        public void QueuedActions_AreAppliedOnTheNextTick()
        {
            _actions.Press(PlayerAction.MoveLeft);
            _actions.Press(PlayerAction.Jump);

            _controller.Tick(Step);

            Assert.That(_motor.Lane, Is.Zero);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Jumping));
        }

        [Test]
        public void JumpPressedJustBeforeLanding_FiresOnLanding()
        {
            _actions.Press(PlayerAction.Jump);
            Simulate(_tuning.JumpDuration - _tuning.InputBufferTime * 0.5f);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Jumping));

            _actions.Press(PlayerAction.Jump);
            Simulate(_tuning.InputBufferTime * 0.5f + 3f * Step);

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Jumping), "The buffered jump should have started a second jump.");
            Assert.That(_motor.Height, Is.GreaterThan(0f));
        }

        [Test]
        public void JumpPressedFarTooEarly_IsForgotten()
        {
            _actions.Press(PlayerAction.Jump);
            Simulate(_tuning.JumpDuration * 0.25f);

            _actions.Press(PlayerAction.Jump);
            Simulate(_tuning.JumpDuration);

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
        }

        [Test]
        public void LaneChangeIntoTheEdge_IsNotRemembered()
        {
            _actions.Press(PlayerAction.MoveLeft);
            _actions.Press(PlayerAction.MoveLeft);
            _controller.Tick(Step);
            Assert.That(_motor.Lane, Is.Zero);

            _actions.Press(PlayerAction.MoveRight);
            _controller.Tick(Step);

            Assert.That(_motor.Lane, Is.EqualTo(1), "A rejected move left must not replay after moving right.");
        }

        [Test]
        public void RunStarted_ResetsTheRunner_AndDropsAnyBufferedAction()
        {
            _actions.Press(PlayerAction.MoveRight);
            _actions.Press(PlayerAction.Jump);
            _controller.Tick(Step);
            _actions.Press(PlayerAction.Jump);
            _controller.Tick(Step);

            _bus.Publish(new RunStarted(seed: 7));
            _controller.Tick(Step);

            Assert.That(_motor.Lane, Is.EqualTo(1));
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
        }

        [Test]
        public void Input_IsEnabledOnlyDuringARun()
        {
            Assert.That(_actions.Enabled, Is.True, "The flow was already in a run when the controller was created.");

            _bus.Publish(new GamePhaseChanged(GamePhase.Run, GamePhase.Paused));
            Assert.That(_actions.Enabled, Is.False);

            _bus.Publish(new GamePhaseChanged(GamePhase.Paused, GamePhase.Run));
            Assert.That(_actions.Enabled, Is.True);
        }

        [Test]
        public void HazardHit_KnocksTheRunnerDown_AndFailsTheRun()
        {
            _bus.Publish(new HazardHit());

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Down));
            Assert.That(_flow.FailedRuns, Is.EqualTo(1));
        }

        [Test]
        public void MovesThatTakeEffect_AreAnnounced()
        {
            var moves = new List<RunnerMove>();
            _bus.Subscribe<RunnerMoved>(m => moves.Add(m.Move));

            _actions.Press(PlayerAction.MoveLeft);
            _actions.Press(PlayerAction.Jump);
            _controller.Tick(Step);
            Simulate(_tuning.JumpDuration + 0.1f);
            _actions.Press(PlayerAction.Slide);
            _controller.Tick(Step);

            Assert.That(moves, Is.EqualTo(new[] { RunnerMove.ChangedLane, RunnerMove.Jumped, RunnerMove.Slid }));
        }

        [Test]
        public void AMoveThatDoesNothing_IsNotAnnounced()
        {
            var moves = new List<RunnerMove>();
            _bus.Subscribe<RunnerMoved>(m => moves.Add(m.Move));

            // Already in the left-most lane after the first press; the second has nowhere to go.
            _actions.Press(PlayerAction.MoveLeft);
            _actions.Press(PlayerAction.MoveLeft);
            _controller.Tick(Step);

            Assert.That(moves, Is.EqualTo(new[] { RunnerMove.ChangedLane }));
        }

        [Test]
        public void ABufferedJump_IsAnnouncedWhenItFires_NotWhenItWasPressed()
        {
            var moves = new List<RunnerMove>();
            _actions.Press(PlayerAction.Jump);
            Simulate(_tuning.JumpDuration - _tuning.InputBufferTime * 0.5f);
            _bus.Subscribe<RunnerMoved>(m => moves.Add(m.Move));

            _actions.Press(PlayerAction.Jump);
            _controller.Tick(Step);
            Assert.That(moves, Is.Empty);

            Simulate(_tuning.InputBufferTime * 0.5f + 3f * Step);
            Assert.That(moves, Is.EqualTo(new[] { RunnerMove.Jumped }));
        }

        [Test]
        public void ClearingTheStage_StandsTheRunnerBackUpInTheMiddle()
        {
            _actions.Press(PlayerAction.MoveRight);
            _controller.Tick(Step);
            _bus.Publish(new HazardHit());
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Down));

            _bus.Publish(new StageCleared());

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
            Assert.That(_motor.Lane, Is.EqualTo(1));
            Assert.That(_motor.X, Is.Zero);
        }

        [Test]
        public void AfterDispose_MessagesAreIgnored()
        {
            _controller.Dispose();

            _bus.Publish(new HazardHit());

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
            Assert.That(_flow.FailedRuns, Is.Zero);
        }

        [Test]
        public void MovingJumpingAndSliding_AllocatesNothing()
        {
            void PlayOneSecond()
            {
                _actions.Press(PlayerAction.MoveLeft);
                _actions.Press(PlayerAction.Jump);
                for (int i = 0; i < 40; i++) _controller.Tick(Step);
                _actions.Press(PlayerAction.Slide);
                _actions.Press(PlayerAction.MoveRight);
                for (int i = 0; i < 80; i++) _controller.Tick(Step);
            }

            PlayOneSecond();

            Assert.That(() => PlayOneSecond(), Is.Not.AllocatingGCMemory());
        }
    }
}
