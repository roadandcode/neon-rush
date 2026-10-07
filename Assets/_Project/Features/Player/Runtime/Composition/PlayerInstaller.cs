using RoadAndCode.Core.Platforms;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Input;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Player.Presentation;
using RoadAndCode.NeonRush.Shared.Input;
using RoadAndCode.NeonRush.Shared.Run;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Player.Composition
{
    /// <summary>
    /// Registers the runner. Needs an ILaneLayout, an IGameFlow, an IInputProfile, an IScreenMetrics and
    /// the InputActionAsset from the scope.
    /// </summary>
    public sealed class PlayerInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private PlayerTuningAsset _tuning;
        [SerializeField] private PlayerView _view;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_tuning.Tuning);
            builder.RegisterComponent(_view);

            builder.Register<PlayerMotor>(Lifetime.Singleton).AsSelf().As<IRunnerBody>();
            // Which input sources exist is decided by the platform's input profile, not by this feature.
            builder.Register<IPlayerActionSource>(
                resolver => PlayerInputFactory.Create(
                    resolver.Resolve<IInputProfile>(),
                    resolver.Resolve<InputActionAsset>(),
                    resolver.Resolve<IScreenMetrics>(),
                    _tuning.Tuning),
                Lifetime.Singleton);

            // Controller before presenter: the pose shown is the one just simulated.
            builder.Register<PlayerController>(Lifetime.Singleton).As<ISimulationSystem>();
            builder.Register<PlayerPresenter>(Lifetime.Singleton).As<ISimulationSystem>();
        }

        private void OnValidate()
        {
            if (_tuning == null) Debug.LogError($"{nameof(PlayerInstaller)} on '{name}' has no tuning asset.", this);
            if (_view == null) Debug.LogError($"{nameof(PlayerInstaller)} on '{name}' has no view.", this);
        }
    }
}
