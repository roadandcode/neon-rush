using RoadAndCode.NeonRush.Cameras.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Presentation
{
    /// <summary>Puts the camera where it is told to. Holds no rules.</summary>
    [DisallowMultipleComponent]
    public sealed class CameraRigView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        /// <summary>Width over height of what the camera is drawing to.</summary>
        public float Aspect => _camera.aspect;

        public void Apply(in CameraPose pose)
        {
            _camera.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            _camera.fieldOfView = pose.FieldOfView;
        }

        private void OnValidate()
        {
            if (_camera == null) Debug.LogError($"{nameof(CameraRigView)} on '{name}' has no camera.", this);
        }
    }
}
