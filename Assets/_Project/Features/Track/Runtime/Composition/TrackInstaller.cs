using RoadAndCode.Core.Randomness;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Track.Data;
using RoadAndCode.NeonRush.Track.Logic;
using RoadAndCode.NeonRush.Track.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Track.Composition
{
    /// <summary>
    /// Registers the track. Needs an ILaneLayout, an IRunProgress and an IRunnerBody from the scope,
    /// so install it after Pace and Player.
    /// </summary>
    public sealed class TrackInstaller : MonoBehaviour, IInstaller
    {
        // Replaced by the run's seed before anything is spawned.
        private const int PlaceholderSeed = 1;

        [SerializeField] private TrackSettingsAsset _settings;
        [SerializeField] private GroundView _ground;

        [Tooltip("Pooled entity views are created under this transform.")]
        [SerializeField] private Transform _entityRoot;

        public void Install(IContainerBuilder builder)
        {
            // The track has its own random stream, so nothing else can disturb a seeded layout.
            var random = new SeededRandom(PlaceholderSeed);

            builder.RegisterInstance(_settings.Settings);
            builder.Register<TrackField>(Lifetime.Singleton);
            builder.Register<IPatternPicker, WeightedPatternPicker>(Lifetime.Singleton)
                .WithParameter(_settings.Patterns)
                .WithParameter<IRandom>(random);
            builder.Register<TrackSpawner>(Lifetime.Singleton);

            // Simulation before presenter: views show the positions just simulated.
            builder.Register<TrackSimulation>(Lifetime.Singleton)
                .WithParameter(random)
                .As<ISimulationSystem>();
            builder.Register<TrackPresenter>(Lifetime.Singleton)
                .WithParameter(_ground)
                .WithParameter(_entityRoot)
                .As<ISimulationSystem>();
        }

        private void OnValidate()
        {
            if (_settings == null) Debug.LogError($"{nameof(TrackInstaller)} on '{name}' has no settings asset.", this);
            if (_ground == null) Debug.LogError($"{nameof(TrackInstaller)} on '{name}' has no ground view.", this);
            if (_entityRoot == null) Debug.LogError($"{nameof(TrackInstaller)} on '{name}' has no entity root.", this);
        }
    }
}
