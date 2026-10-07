using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Presentation
{
    /// <summary>The visible part of a track entity. Pooled, and placed by the presenter every tick.</summary>
    public sealed class TrackEntityView : MonoBehaviour
    {
        [Tooltip("Optional part that rotates, for pickups.")]
        [SerializeField] private Transform _spinner;

        public void Place(float x, float z, float spinDegrees)
        {
            transform.localPosition = new Vector3(x, 0f, z);
            if (_spinner != null) _spinner.localRotation = Quaternion.Euler(0f, spinDegrees, 0f);
        }
    }
}
