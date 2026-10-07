using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Input;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Player.Presentation;
using RoadAndCode.NeonRush.Shared.Run;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Player.Composition
{
    /// <summary>
    /// Registers the runner. Needs an ILaneLayout, an IGameFlow and the InputActionAsset from the scope.
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
            builder.Register<IPlayerActionSource, InputSystemActionSource>(Lifetime.Singleton);

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
