using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Presentation
{
    /// <summary>Draws the runner where it is told to. Holds no rules.</summary>
    public sealed class PlayerView : MonoBehaviour
    {
        [Tooltip("Scaled and rolled for slide and lean. Its pivot should be at the feet.")]
        [SerializeField] private Transform _body;

        public void Render(in PlayerPose pose)
        {
            transform.localPosition = new Vector3(pose.X, pose.Height, 0f);
            _body.localScale = new Vector3(1f, pose.Stature, 1f);
            _body.localRotation = Quaternion.Euler(-pose.Tilt, 0f, -pose.Lean);
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogError($"{nameof(PlayerView)} on '{name}' has no body transform.", this);
        }
    }
}
