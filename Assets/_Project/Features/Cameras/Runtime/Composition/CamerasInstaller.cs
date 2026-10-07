using RoadAndCode.NeonRush.Cameras.Data;
using RoadAndCode.NeonRush.Cameras.Logic;
using RoadAndCode.NeonRush.Cameras.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Cameras.Composition
{
    /// <summary>Registers the camera: its shots, the director that chooses between them and the rig. Needs an IGameFlow and the message bus from the scope.</summary>
    public sealed class CamerasInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private CameraSettingsAsset _settings;
        [SerializeField] private CameraRigView _rig;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings.Settings);
            builder.RegisterComponent(_rig);
            builder.Register<CameraDirector>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CameraDriver>();
        }

        private void OnValidate()
        {
            if (_settings == null) Debug.LogError($"{nameof(CamerasInstaller)} on '{name}' has no settings asset.", this);
            if (_rig == null) Debug.LogError($"{nameof(CamerasInstaller)} on '{name}' has no camera rig.", this);
        }
    }
}
