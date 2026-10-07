using RoadAndCode.NeonRush.Cameras.Data;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Logic
{
    /// <summary>Turns a shot into a camera position and rotation. Pure maths, so it is tested without a camera.</summary>
    internal static class ShotSolver
    {
        private const float Smallest = 0.0001f;

        public static CameraPose Solve(in CameraShot shot, Vector3 subject, float aspect)
        {
            Vector3 position = subject + Quaternion.Euler(0f, shot.Azimuth, 0f) * new Vector3(0f, shot.Height, -shot.Distance);
            Quaternion rotation = Aim(subject + shot.LookAt - position, shot.Framing, shot.FieldOfView, aspect);
            return new CameraPose(position, rotation, shot.FieldOfView);
        }

        /// <summary>
        /// The rotation, with no roll, that puts a target <paramref name="framing"/> of the screen's
        /// width to the right of centre and exactly half-way up.
        ///
        /// Framing is a share of the screen, so the turn needed depends on the aspect ratio; solving
        /// it here is what lets one shot work in a wide window and a nearly square one. Turning a
        /// camera that already looks at the target would do it in one line, but that turn is about
        /// the camera's tilted up axis and leaves the horizon sloping, so yaw and pitch are solved
        /// together instead.
        /// </summary>
        public static Quaternion Aim(Vector3 toTarget, float framing, float verticalFieldOfView, float aspect)
        {
            float flat = Mathf.Sqrt(toTarget.x * toTarget.x + toTarget.z * toTarget.z);
            if (flat < Smallest) return Quaternion.LookRotation(toTarget, Vector3.forward);

            // Tangent of the angle, seen from the camera, between straight ahead and the target.
            float halfWidth = Mathf.Tan(verticalFieldOfView * 0.5f * Mathf.Deg2Rad) * aspect;
            float tangent = 2f * framing * halfWidth;

            // How far the target's compass bearing is from the camera's. For a camera looking level
            // this is the same angle; the more it tilts, the more the bearing has to open up.
            float cosineSquared = (flat * flat - tangent * tangent * toTarget.y * toTarget.y) / (flat * flat * (1f + tangent * tangent));
            float bearingOffset = Mathf.Acos(Mathf.Sqrt(Mathf.Clamp01(cosineSquared))) * Mathf.Sign(tangent);

            float yaw = Mathf.Atan2(toTarget.x, toTarget.z) - bearingOffset;
            float pitch = Mathf.Atan2(-toTarget.y, flat * Mathf.Cos(bearingOffset));
            return Quaternion.Euler(pitch * Mathf.Rad2Deg, yaw * Mathf.Rad2Deg, 0f);
        }
    }
}
