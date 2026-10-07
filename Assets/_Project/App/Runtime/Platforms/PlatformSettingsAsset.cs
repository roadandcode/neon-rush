using RoadAndCode.Core.Platforms;
using UnityEngine;

namespace RoadAndCode.NeonRush.App.Platforms
{
    /// <summary>Per-platform runtime settings, one section per platform family.</summary>
    [CreateAssetMenu(menuName = "Neon Rush/Platform Settings", fileName = "PlatformSettings")]
    internal sealed class PlatformSettingsAsset : ScriptableObject
    {
        [SerializeField] private PerPlatform<PlatformSettings> _settings;

        public PlatformSettings For(PlatformKind kind) => _settings.For(kind);
    }
}
