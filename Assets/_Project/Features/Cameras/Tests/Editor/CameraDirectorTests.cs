using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Cameras.Data;
using RoadAndCode.NeonRush.Cameras.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Tests
{
    public sealed class CameraDirectorTests
    {
        private sealed class StubFlow : IGameFlow
        {
            public GamePhase Phase { get; set; } = GamePhase.Boot;

            public bool StartRun() => false;

            public bool Pause() => false;

            public bool Resume() => false;

            public bool FailRun() => false;

            public bool ReturnToMenu() => false;
        }

        private const float Aspect = 16f / 9f;
        private const float Tolerance = 0.001f;
        private const float ReturnSeconds = 1f;
        private const float ShakeSeconds = 0.5f;

        private static readonly CameraShot MenuShot = new CameraShot(160f, 6f, 1.5f, new Vector3(0f, 1f, 0f), 40f, 0.2f);
        private static readonly CameraShot RunShot = new CameraShot(0f, 10f, 5f, new Vector3(0f, 1f, 4f), 60f, 0f);

        private StubFlow _flow;
        private MessageBus _bus;
        private CameraDirector _director;

        [SetUp]
        public void SetUp()
        {
            _flow = new StubFlow();
            _bus = new MessageBus();
            _director = new CameraDirector(
                new CameraSettings(MenuShot, RunShot, ReturnSeconds, ShakeSeconds, shakeDistance: 0.5f, shakeFrequency: 10f),
                _flow,
                _bus);
        }

        private void Enter(GamePhase phase)
        {
            GamePhase previous = _flow.Phase;
            _flow.Phase = phase;
            _bus.Publish(new GamePhaseChanged(previous, phase));
        }

        private void AssertOn(CameraShot expected)
        {
            CameraShot shot = _director.Shot;
            Assert.That(shot.Azimuth, Is.EqualTo(expected.Azimuth).Within(Tolerance), "azimuth");
            Assert.That(shot.Distance, Is.EqualTo(expected.Distance).Within(Tolerance), "distance");
            Assert.That(shot.Height, Is.EqualTo(expected.Height).Within(Tolerance), "height");
            Assert.That(shot.FieldOfView, Is.EqualTo(expected.FieldOfView).Within(Tolerance), "field of view");
            Assert.That(shot.Framing, Is.EqualTo(expected.Framing).Within(Tolerance), "framing");
        }

        [Test]
        public void TheGameOpens_OnTheMenuShot_WithoutAMove()
        {
            Enter(GamePhase.Menu);

            Assert.That(_director.IsMoving, Is.False);
            AssertOn(MenuShot);
        }

        [Test]
        public void TheIntro_CarriesTheCameraToTheRunShot_InExactlyItsLength()
        {
            Enter(GamePhase.Menu);
            Enter(GamePhase.Intro);
            _bus.Publish(new RunIntroStarted(2f));

            _director.Tick(1f);
            Assert.That(_director.IsMoving, Is.True);
            Assert.That(_director.Shot.Azimuth, Is.EqualTo(80f).Within(Tolerance), "Half-way in time is half-way round.");

            _director.Tick(1f);
            Assert.That(_director.IsMoving, Is.False);
            AssertOn(RunShot);
        }

        [Test]
        public void TheMove_StartsAndEndsGently()
        {
            Enter(GamePhase.Menu);
            _bus.Publish(new RunIntroStarted(1f));

            _director.Tick(0.1f);
            float early = MenuShot.Azimuth - _director.Shot.Azimuth;
            _director.Tick(0.4f);
            float middle = MenuShot.Azimuth - _director.Shot.Azimuth;

            Assert.That(early, Is.GreaterThan(0f));
            Assert.That(early, Is.LessThan(160f * 0.05f), "A tenth of the time should cover well under a tenth of the way.");
            Assert.That(middle, Is.EqualTo(80f).Within(Tolerance));
        }

        [Test]
        public void ARunWithNoIntro_CutsStraightToTheRunShot()
        {
            Enter(GamePhase.Menu);

            Enter(GamePhase.Run);

            Assert.That(_director.IsMoving, Is.False);
            AssertOn(RunShot);
        }

        [Test]
        public void GoingBackToTheMenu_SwingsTheCameraRound()
        {
            Enter(GamePhase.Menu);
            Enter(GamePhase.Run);
            Enter(GamePhase.GameOver);

            Enter(GamePhase.Menu);
            Assert.That(_director.IsMoving, Is.True);
            AssertOn(RunShot);

            _director.Tick(ReturnSeconds);
            Assert.That(_director.IsMoving, Is.False);
            AssertOn(MenuShot);
        }

        [Test]
        public void AMoveThatInterruptsAnother_StartsFromWhereTheCameraIs()
        {
            Enter(GamePhase.Menu);
            Enter(GamePhase.Run);
            Enter(GamePhase.GameOver);
            Enter(GamePhase.Menu);
            _director.Tick(ReturnSeconds * 0.5f);
            float azimuthWhenInterrupted = _director.Shot.Azimuth;

            _bus.Publish(new RunIntroStarted(1f));

            Assert.That(_director.Shot.Azimuth, Is.EqualTo(azimuthWhenInterrupted).Within(Tolerance));
            _director.Tick(1f);
            AssertOn(RunShot);
        }

        [Test]
        public void ARetryAfterACrash_LeavesTheCameraWhereItIs()
        {
            Enter(GamePhase.Menu);
            Enter(GamePhase.Run);
            Enter(GamePhase.GameOver);

            Enter(GamePhase.Run);

            Assert.That(_director.IsMoving, Is.False);
            AssertOn(RunShot);
        }

        [Test]
        public void ADirectorCreatedMidRun_StartsOnTheRunShot()
        {
            _flow.Phase = GamePhase.Run;

            using var late = new CameraDirector(new CameraSettings(MenuShot, RunShot, 1f, 0.5f, 0.5f, 10f), _flow, _bus);

            Assert.That(late.Shot.Azimuth, Is.EqualTo(RunShot.Azimuth).Within(Tolerance));
        }

        [Test]
        public void AHit_ShakesTheCamera_ThenLetsItSettle()
        {
            Enter(GamePhase.Menu);
            Enter(GamePhase.Run);
            Vector3 still = _director.Pose(Aspect).Position;

            _bus.Publish(new HazardHit());
            _director.Tick(0.02f);
            Assert.That(Vector3.Distance(_director.Pose(Aspect).Position, still), Is.GreaterThan(0.05f));

            _director.Tick(ShakeSeconds);
            Assert.That(Vector3.Distance(_director.Pose(Aspect).Position, still), Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void TheShake_MovesTheCameraButNotWhereItPoints()
        {
            Enter(GamePhase.Menu);
            Enter(GamePhase.Run);
            Quaternion still = _director.Pose(Aspect).Rotation;

            _bus.Publish(new HazardHit());
            _director.Tick(0.02f);

            Assert.That(Quaternion.Angle(_director.Pose(Aspect).Rotation, still), Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void AfterDispose_TheCameraStaysPut()
        {
            Enter(GamePhase.Menu);
            _director.Dispose();

            _bus.Publish(new RunIntroStarted(1f));
            _director.Tick(1f);

            AssertOn(MenuShot);
        }
    }
}
