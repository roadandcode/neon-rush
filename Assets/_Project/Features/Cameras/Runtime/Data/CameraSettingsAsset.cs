using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Data
{
    [CreateAssetMenu(menuName = "Neon Rush/Camera Settings", fileName = "CameraSettings")]
    internal sealed class CameraSettingsAsset : ScriptableObject
    {
        [SerializeField] private CameraSettings _settings = new CameraSettings();

        public CameraSettings Settings => _settings;
    }
}
