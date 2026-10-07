using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Logic
{
    /// <summary>Everything the camera needs for one frame.</summary>
    public readonly struct CameraPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float FieldOfView;

        public CameraPose(Vector3 position, Quaternion rotation, float fieldOfView)
        {
            Position = position;
            Rotation = rotation;
            FieldOfView = fieldOfView;
        }
    }
}
