using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.App.Platforms
{
    /// <summary>How the game should run on one platform family.</summary>
    [Serializable]
    internal sealed class PlatformSettings
    {
        public const int PlatformDefaultFrameRate = -1;

        [Tooltip("Frames per second to aim for. -1 leaves it to the platform: the display's refresh rate on desktop, the browser on web.")]
        [SerializeField] private int _targetFrameRate = PlatformDefaultFrameRate;

        [SerializeField] private bool _vSync;

        [Tooltip("Stop the device dimming or locking the screen during play. Matters on phones, where there may be no touch for a while.")]
        [SerializeField] private bool _keepScreenAwake;

        public int TargetFrameRate => _targetFrameRate;
        public bool VSync => _vSync;
        public bool KeepScreenAwake => _keepScreenAwake;
    }
}
