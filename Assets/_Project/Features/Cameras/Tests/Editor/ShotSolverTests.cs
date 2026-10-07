using NUnit.Framework;
using RoadAndCode.NeonRush.Cameras.Data;
using RoadAndCode.NeonRush.Cameras.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Tests
{
    public sealed class ShotSolverTests
    {
        private const float Tolerance = 0.001f;

        private static readonly Vector3 Subject = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 Chest = new Vector3(0f, 1f, 0f);

        private static CameraShot Shot(float azimuth, float framing = 0f, float fieldOfView = 60f)
        {
            return new CameraShot(azimuth, 8f, 2f, Chest, fieldOfView, framing);
        }

        /// <summary>Where a world point lands across the screen: 0 is the centre, 0.5 the right edge.</summary>
        private static float ScreenX(in CameraPose pose, Vector3 point, float aspect)
        {
            Vector3 local = Quaternion.Inverse(pose.Rotation) * (point - pose.Position);
            float halfWidth = Mathf.Tan(pose.FieldOfView * 0.5f * Mathf.Deg2Rad) * aspect;
            return local.x / local.z / halfWidth * 0.5f;
        }

        private static float ScreenY(in CameraPose pose, Vector3 point)
        {
            Vector3 local = Quaternion.Inverse(pose.Rotation) * (point - pose.Position);
            return local.y / local.z / Mathf.Tan(pose.FieldOfView * 0.5f * Mathf.Deg2Rad) * 0.5f;
        }

        [Test]
        public void AzimuthZero_IsDirectlyBehindTheSubject()
        {
            CameraPose pose = ShotSolver.Solve(Shot(azimuth: 0f), Subject, aspect: 16f / 9f);

            Assert.That(pose.Position.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(pose.Position.y, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(pose.Position.z, Is.EqualTo(-8f).Within(Tolerance));
        }

        [Test]
        public void Azimuth180_IsDirectlyInFront_LookingBack()
        {
            CameraPose pose = ShotSolver.Solve(Shot(azimuth: 180f), Subject, aspect: 16f / 9f);

            Assert.That(pose.Position.z, Is.EqualTo(8f).Within(Tolerance));
            Assert.That((pose.Rotation * Vector3.forward).z, Is.LessThan(0f));
        }

        [Test]
        public void TheSubjectMovesWithTheShot()
        {
            var subject = new Vector3(2.5f, 0f, 10f);

            CameraPose pose = ShotSolver.Solve(Shot(azimuth: 0f), subject, aspect: 16f / 9f);

            Assert.That(pose.Position.x, Is.EqualTo(2.5f).Within(Tolerance));
            Assert.That(pose.Position.z, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void WithNoFraming_TheTargetIsDeadCentre()
        {
            CameraPose pose = ShotSolver.Solve(Shot(azimuth: 140f), Subject, aspect: 16f / 9f);

            Assert.That(ScreenX(pose, Chest, 16f / 9f), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(ScreenY(pose, Chest), Is.EqualTo(0f).Within(Tolerance));
        }

        [TestCase(16f / 9f)]
        [TestCase(21f / 9f)]
        [TestCase(4f / 3f)]
        [TestCase(1f)]
        [TestCase(9f / 16f)]
        public void Framing_PutsTheTargetTheSameShareAcrossTheScreen_AtAnyAspect(float aspect)
        {
            CameraPose pose = ShotSolver.Solve(Shot(azimuth: 162f, framing: 0.22f, fieldOfView: 38f), Subject, aspect);

            Assert.That(ScreenX(pose, Chest, aspect), Is.EqualTo(0.22f).Within(Tolerance));
            Assert.That(ScreenY(pose, Chest), Is.EqualTo(0f).Within(Tolerance), "Framing should slide the target sideways only.");
        }

        [TestCase(0f)]
        [TestCase(0.22f)]
        [TestCase(-0.4f)]
        public void TheHorizonStaysLevel_HoweverTheShotIsFramed(float framing)
        {
            // High up and looking steeply down: the case where an off-centre target tempts the camera to roll.
            var shot = new CameraShot(150f, 5f, 6f, Chest, 50f, framing);

            CameraPose pose = ShotSolver.Solve(shot, Subject, aspect: 16f / 9f);

            Assert.That((pose.Rotation * Vector3.right).y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(ScreenX(pose, Chest, 16f / 9f), Is.EqualTo(framing).Within(Tolerance));
            Assert.That(ScreenY(pose, Chest), Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void NegativeFraming_PutsTheTargetOnTheLeft()
        {
            CameraPose pose = ShotSolver.Solve(Shot(azimuth: 0f, framing: -0.3f), Subject, aspect: 16f / 9f);

            Assert.That(ScreenX(pose, Chest, 16f / 9f), Is.EqualTo(-0.3f).Within(Tolerance));
        }

        [Test]
        public void BlendingShots_GoesRoundTheSubject_NotThroughIt()
        {
            CameraShot front = Shot(azimuth: 180f);
            CameraShot behind = Shot(azimuth: 0f);

            CameraPose halfway = ShotSolver.Solve(CameraShot.Lerp(front, behind, 0.5f), Subject, aspect: 16f / 9f);

            // A straight line between the two would pass over the subject's head. The orbit is out to the side.
            var flat = new Vector2(halfway.Position.x, halfway.Position.z);
            Assert.That(flat.magnitude, Is.EqualTo(8f).Within(Tolerance));
            Assert.That(Mathf.Abs(halfway.Position.x), Is.EqualTo(8f).Within(Tolerance));
        }
    }
}
