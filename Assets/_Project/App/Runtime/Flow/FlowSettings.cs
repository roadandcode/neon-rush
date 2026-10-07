using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>Timings of the game flow itself.</summary>
    [Serializable]
    internal sealed class FlowSettings
    {
        [Tooltip("Seconds between pressing play on the menu and the run starting. The camera move and the start sound last this long. Zero starts the run at once.")]
        [SerializeField, Min(0f)] private float _introSeconds = 1.6f;

        public FlowSettings()
        {
        }

        public FlowSettings(float introSeconds)
        {
            _introSeconds = introSeconds;
        }

        public float IntroSeconds => _introSeconds;
    }
}
