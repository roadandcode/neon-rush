using UnityEngine;

namespace RoadAndCode.Core.Platforms
{
    public sealed class UnityScreenMetrics : IScreenMetrics
    {
        public float ShortSide => Mathf.Min(Screen.width, Screen.height);
    }
}
