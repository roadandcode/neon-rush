using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Pace.Composition;
using RoadAndCode.NeonRush.Player.Composition;
using RoadAndCode.NeonRush.Scoring.Composition;
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
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Features")]
        [SerializeField] private PaceInstaller _pace;
        [SerializeField] private PlayerInstaller _player;
        [SerializeField] private TrackInstaller _track;
        [SerializeField] private ScoringInstaller _scoring;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPointExceptionHandler(Debug.LogException);

            builder.RegisterInstance(_lanes.CreateGrid());
            builder.RegisterInstance(_inputActions);

            // Install order is tick order, because simulation systems run in the order they register:
            // the clock, then the runner, then the track (which checks contacts against where the
            // runner now is), then the score (which counts what was just collected).
            _pace.Install(builder);
            _player.Install(builder);
            _track.Install(builder);
            _scoring.Install(builder);

            builder.Register<SimulationLoop>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SimulationDriver>();
            builder.RegisterEntryPoint<FlowInput>();
        }
    }
}
