using NUnit.Framework;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Tests
{
    public sealed class PlayerMotorTests
    {
        private const float Step = 1f / 120f;
        private const float Tolerance = 1e-3f;

        private PlayerTuning _tuning;
        private LaneGrid _lanes;
        private PlayerMotor _motor;

        [SetUp]
        public void SetUp()
        {
            _tuning = new PlayerTuning();
            _lanes = new LaneGrid(laneCount: 3, laneWidth: 2.5f);
            _motor = new PlayerMotor(_tuning, _lanes);
        }

        private void Simulate(float seconds)
        {
            for (float t = 0f; t < seconds; t += Step) _motor.Tick(Step);
        }

        [Test]
        public void StartsRunningInTheCentreLaneOnTheGround()
        {
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
            Assert.That(_motor.Lane, Is.EqualTo(_lanes.CenterLane));
            Assert.That(_motor.X, Is.EqualTo(_lanes.CenterOf(_lanes.CenterLane)));
            Assert.That(_motor.Height, Is.Zero);
        }

        [Test]
        public void LaneChange_TargetsTheNextLaneImmediately_AndTravelsThereAtLaneChangeSpeed()
        {
            Assert.That(_motor.Apply(PlayerAction.MoveRight), Is.True);
            Assert.That(_motor.Lane, Is.EqualTo(_lanes.CenterLane + 1));

            _motor.Tick(0.05f);
            Assert.That(_motor.X, Is.EqualTo(_tuning.LaneChangeSpeed * 0.05f).Within(Tolerance));

            Simulate(_lanes.LaneWidth / _tuning.LaneChangeSpeed + 0.05f);
            Assert.That(_motor.X, Is.EqualTo(_lanes.CenterOf(_lanes.CenterLane + 1)).Within(Tolerance));
        }

        [Test]
        public void LaneChange_PastTheEdgeOfTheTrack_IsRejected()
        {
            _motor.Apply(PlayerAction.MoveLeft);

            Assert.That(_motor.Apply(PlayerAction.MoveLeft), Is.False);
            Assert.That(_motor.Lane, Is.Zero);
        }

        [Test]
        public void Jump_PeaksAtJumpHeightHalfwayThrough_AndLandsAfterJumpDuration()
        {
            Assert.That(_motor.Apply(PlayerAction.Jump), Is.True);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Jumping));

            _motor.Tick(_tuning.JumpDuration * 0.5f);
            Assert.That(_motor.Height, Is.EqualTo(_tuning.JumpHeight).Within(Tolerance));

            _motor.Tick(_tuning.JumpDuration * 0.5f + Step);
            Assert.That(_motor.Height, Is.Zero);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
        }

        [Test]
        public void Jump_WhileAlreadyInTheAir_IsRejected()
        {
            _motor.Apply(PlayerAction.Jump);
            _motor.Tick(0.1f);

            Assert.That(_motor.Apply(PlayerAction.Jump), Is.False);
        }

        [Test]
        public void LaneChange_IsAllowedInTheAir()
        {
            _motor.Apply(PlayerAction.Jump);
            _motor.Tick(0.1f);

            Assert.That(_motor.Apply(PlayerAction.MoveLeft), Is.True);
        }

        [Test]
        public void Slide_LowersTheBody_AndEndsAfterSlideDuration()
        {
            Assert.That(_motor.Apply(PlayerAction.Slide), Is.True);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Sliding));
            Assert.That(_motor.BodyHeight, Is.EqualTo(_tuning.SlidingHeight));
            Assert.That(_motor.Bounds.max.y, Is.EqualTo(_tuning.SlidingHeight).Within(Tolerance));

            Simulate(_tuning.SlideDuration + Step);

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
            Assert.That(_motor.BodyHeight, Is.EqualTo(_tuning.StandingHeight));
        }

        [Test]
        public void Jump_CancelsASlide()
        {
            _motor.Apply(PlayerAction.Slide);
            _motor.Tick(0.1f);

            Assert.That(_motor.Apply(PlayerAction.Jump), Is.True);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Jumping));
        }

        [Test]
        public void Slide_InTheAir_DropsFast_AndLandsInASlide()
        {
            _motor.Apply(PlayerAction.Jump);
            _motor.Tick(_tuning.JumpDuration * 0.5f);
            float apex = _motor.Height;

            Assert.That(_motor.Apply(PlayerAction.Slide), Is.True);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Jumping), "Still airborne until it lands.");

            float fallTime = apex / _tuning.DiveSpeed;
            Assert.That(fallTime, Is.LessThan(_tuning.JumpDuration * 0.5f), "A dive must beat the normal descent.");

            Simulate(fallTime + 2f * Step);
            Assert.That(_motor.Height, Is.Zero);
            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Sliding));
        }

        [Test]
        public void Bounds_FollowPositionHeightAndStature()
        {
            _motor.Apply(PlayerAction.Jump);
            _motor.Tick(_tuning.JumpDuration * 0.5f);

            Bounds bounds = _motor.Bounds;

            Assert.That(bounds.min.y, Is.EqualTo(_motor.Height).Within(Tolerance));
            Assert.That(bounds.size, Is.EqualTo(new Vector3(_tuning.Width, _tuning.StandingHeight, _tuning.Depth)));
            Assert.That(bounds.center.x, Is.EqualTo(_motor.X).Within(Tolerance));
            Assert.That(bounds.center.z, Is.Zero);
        }

        [Test]
        public void KnockedDown_IgnoresActionsAndStopsMoving()
        {
            _motor.Apply(PlayerAction.MoveRight);
            _motor.KnockDown();
            float x = _motor.X;

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Down));
            Assert.That(_motor.Apply(PlayerAction.Jump), Is.False);
            Assert.That(_motor.Apply(PlayerAction.MoveLeft), Is.False);

            _motor.Tick(0.5f);
            Assert.That(_motor.X, Is.EqualTo(x));
        }

        [Test]
        public void Reset_ReturnsToTheStartFromAnyMode()
        {
            _motor.Apply(PlayerAction.MoveRight);
            _motor.Apply(PlayerAction.Jump);
            _motor.Tick(0.2f);
            _motor.KnockDown();

            _motor.Reset();

            Assert.That(_motor.Mode, Is.EqualTo(PlayerMode.Running));
            Assert.That(_motor.Lane, Is.EqualTo(_lanes.CenterLane));
            Assert.That(_motor.X, Is.EqualTo(_lanes.CenterOf(_lanes.CenterLane)));
            Assert.That(_motor.Height, Is.Zero);
        }
    }
}
