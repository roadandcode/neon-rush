using UnityEngine;

namespace RoadAndCode.Core.Platforms
{
    /// <summary>The platform the player is actually running on.</summary>
    public sealed class RuntimePlatformService : IPlatform
    {
        public PlatformKind Kind => KindOf(Application.platform);

        public static PlatformKind KindOf(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WebGLPlayer:
                    return PlatformKind.Web;
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    return PlatformKind.Mobile;
                default:
                    return PlatformKind.Desktop;
            }
        }
    }
}
