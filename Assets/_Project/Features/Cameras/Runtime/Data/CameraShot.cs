using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Data
{
    /// <summary>
    /// Where the camera sits and what it looks at, described as an orbit around the subject
    /// rather than as a position. Blending two shots then swings the camera around the subject
    /// instead of dragging it in a straight line through it.
    /// </summary>
    [Serializable]
    internal struct CameraShot
    {
        [Tooltip("Degrees around the subject. 0 is directly behind it, 180 directly in front.")]
        [SerializeField] private float _azimuth;

        [Tooltip("Horizontal distance from the subject in metres.")]
        [SerializeField, Min(0.1f)] private float _distance;

        [Tooltip("Height of the camera above the subject's feet in metres.")]
        [SerializeField] private float _height;

        [Tooltip("The point the camera looks at, relative to the subject's feet.")]
        [SerializeField] private Vector3 _lookAt;

        [SerializeField, Range(10f, 100f)] private float _fieldOfView;

        [Tooltip("Where the look-at point sits across the screen. 0 is the centre, 0.25 is halfway to the right edge, negative is left. Holds at any aspect ratio.")]
        [SerializeField, Range(-0.45f, 0.45f)] private float _framing;

        public CameraShot(float azimuth, float distance, float height, Vector3 lookAt, float fieldOfView, float framing)
        {
            _azimuth = azimuth;
            _distance = distance;
            _height = height;
            _lookAt = lookAt;
            _fieldOfView = fieldOfView;
            _framing = framing;
        }

        public float Azimuth => _azimuth;
        public float Distance => _distance;
        public float Height => _height;
        public Vector3 LookAt => _lookAt;
        public float FieldOfView => _fieldOfView;
        public float Framing => _framing;

        /// <summary>
        /// Azimuth is blended as a plain number, not as a wrapped angle, so the two shots decide
        /// which way round the camera travels: 160 to 0 goes one way, -200 to 0 the other.
        /// </summary>
        public static CameraShot Lerp(in CameraShot from, in CameraShot to, float t)
        {
            return new CameraShot(
                Mathf.Lerp(from._azimuth, to._azimuth, t),
                Mathf.Lerp(from._distance, to._distance, t),
                Mathf.Lerp(from._height, to._height, t),
                Vector3.Lerp(from._lookAt, to._lookAt, t),
                Mathf.Lerp(from._fieldOfView, to._fieldOfView, t),
                Mathf.Lerp(from._framing, to._framing, t));
        }
    }
}
