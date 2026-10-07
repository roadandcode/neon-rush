using RoadAndCode.Core.Platforms;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.App.Input;
using RoadAndCode.NeonRush.Pace.Composition;
using RoadAndCode.NeonRush.Player.Composition;
using RoadAndCode.NeonRush.Scoring.Composition;
using RoadAndCode.NeonRush.Screens.Composition;
using RoadAndCode.NeonRush.Shared.Input;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Composition;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>
    /// Composition root for the gameplay scene. Each feature registers itself through its
    /// installer; this class decides which features exist and in what order.
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Shared")]
        [SerializeField] private LaneLayoutAsset _lanes;

        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Tooltip("Which control schemes each platform uses.")]
        [SerializeField] private PerPlatform<InputProfileAsset> _inputProfiles;

        [Header("Features")]
        [SerializeField] private PaceInstaller _pace;
        [SerializeField] private PlayerInstaller _player;
        [SerializeField] private TrackInstaller _track;
        [SerializeField] private ScoringInstaller _scoring;
        [SerializeField] private ScreensInstaller _screens;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPointExceptionHandler(Debug.LogException);

            builder.RegisterInstance(_lanes.CreateGrid());
            RegisterInput(builder);

            // Install order is tick order, because simulation systems run in the order they register:
            // the clock, then the runner, then the track (which checks contacts against where the
            // runner now is), then the score (which counts what was just collected).
            _pace.Install(builder);
            _player.Install(builder);
            _track.Install(builder);
            _scoring.Install(builder);

            // Screens are not part of the simulation: they listen to messages and call the flow.
            _screens.Install(builder);

            builder.Register<SimulationLoop>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SimulationDriver>();
            builder.RegisterEntryPoint<FlowInput>();
        }

        // Input, bottom layer up: the actions asset, narrowed to this platform's control schemes.
        // Features then build their own sources on top, from the same profile.
        private void RegisterInput(IContainerBuilder builder)
        {
            builder.Register<IInputProfile>(
                resolver => _inputProfiles.For(resolver.Resolve<IPlatform>().Kind),
                Lifetime.Singleton);
            builder.Register<PlatformInput>(Lifetime.Singleton).WithParameter(_inputActions);
            builder.Register(resolver => resolver.Resolve<PlatformInput>().Actions, Lifetime.Singleton);
        }

        private void OnValidate()
        {
            foreach (PlatformKind kind in System.Enum.GetValues(typeof(PlatformKind)))
            {
                if (_inputProfiles.For(kind) == null)
                {
                    Debug.LogError($"{nameof(GameplayLifetimeScope)} on '{name}' has no input profile for {kind}.", this);
                }
            }
        }
    }
}
