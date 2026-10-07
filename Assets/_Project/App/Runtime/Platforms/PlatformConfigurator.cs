using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Platforms;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Platforms
{
    /// <summary>Applies the current platform's settings once, at start-up.</summary>
    internal sealed class PlatformConfigurator : IStartable
    {
        private readonly IPlatform _platform;
        private readonly PlatformSettingsAsset _settings;

        public PlatformConfigurator(IPlatform platform, PlatformSettingsAsset settings)
        {
            _platform = Guard.NotNull(platform, nameof(platform));
            _settings = Guard.NotNull(settings, nameof(settings));
        }

        public void Start()
        {
            var settings = _settings.For(_platform.Kind);

            // Unity ignores the frame-rate target while vSync is on, so set vSync first.
            QualitySettings.vSyncCount = settings.VSync ? 1 : 0;
            Application.targetFrameRate = settings.TargetFrameRate;
            Screen.sleepTimeout = settings.KeepScreenAwake ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        }
    }
}
