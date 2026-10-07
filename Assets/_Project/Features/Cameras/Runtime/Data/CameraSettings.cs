using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Data
{
    /// <summary>The game's shots and how the camera moves between them.</summary>
    [Serializable]
    internal sealed class CameraSettings
    {
        [Header("Shots")]
        [Tooltip("Title screen: in front of the runner, with the runner to one side so the menu has the other.")]
        [SerializeField] private CameraShot _menu = new CameraShot(162f, 6f, 1.45f, new Vector3(0f, 1f, 0f), 38f, 0.22f);

        [Tooltip("The run: behind and above the runner, looking down the track.")]
        [SerializeField] private CameraShot _run = new CameraShot(0f, 9.5f, 5.6f, new Vector3(0f, 1f, 3.86f), 60f, 0f);

        [Header("Moves")]
        [Tooltip("Seconds to swing back to the menu shot after a run. The move into a run takes its length from the flow's intro.")]
        [SerializeField, Min(0f)] private float _returnSeconds = 0.9f;

        [Header("Impact shake")]
        [SerializeField, Min(0f)] private float _shakeSeconds = 0.45f;

        [Tooltip("How far the camera is thrown at the moment of impact, in metres. It dies away over the shake.")]
        [SerializeField, Min(0f)] private float _shakeDistance = 0.4f;

        [Tooltip("Shakes per second.")]
        [SerializeField, Min(0f)] private float _shakeFrequency = 9f;

        public CameraSettings()
        {
        }

        public CameraSettings(CameraShot menu, CameraShot run, float returnSeconds, float shakeSeconds, float shakeDistance, float shakeFrequency)
        {
            _menu = menu;
            _run = run;
            _returnSeconds = returnSeconds;
            _shakeSeconds = shakeSeconds;
            _shakeDistance = shakeDistance;
            _shakeFrequency = shakeFrequency;
        }

        public CameraShot Menu => _menu;
        public CameraShot Run => _run;
        public float ReturnSeconds => _returnSeconds;
        public float ShakeSeconds => _shakeSeconds;
        public float ShakeDistance => _shakeDistance;
        public float ShakeFrequency => _shakeFrequency;
    }
}
