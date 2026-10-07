using UnityEngine;

namespace RoadAndCode.NeonRush.App.Flow
{
    [CreateAssetMenu(menuName = "Neon Rush/Flow Settings", fileName = "FlowSettings")]
    internal sealed class FlowSettingsAsset : ScriptableObject
    {
        [SerializeField] private FlowSettings _settings = new FlowSettings();

        public FlowSettings Settings => _settings;
    }
}
