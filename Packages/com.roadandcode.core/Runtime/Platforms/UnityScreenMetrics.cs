using UnityEngine;

namespace RoadAndCode.Core.Platforms
{
    public sealed class UnityScreenMetrics : IScreenMetrics, ISafeArea
    {
        public float ShortSide => Mathf.Min(Screen.width, Screen.height);

        public ScreenInsets Insets => ScreenInsets.From(Screen.safeArea, Screen.width, Screen.height);
    }
}
