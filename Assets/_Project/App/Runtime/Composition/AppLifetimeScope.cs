using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.App.Platforms;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>
    /// Composition root for everything that lives as long as the session: the platform, messaging,
    /// saves and the game flow. Lives in the Bootstrap scene. Gameplay is composed by
    /// <see cref="GameplayLifetimeScope"/>, as a child of this scope.
    /// </summary>
    public sealed class AppLifetimeScope : LifetimeScope
    {
        private const string SavePrefix = "neonrush.";

        [Header("Platform")]
        [SerializeField] private PlatformSettingsAsset _platformSettings;

        [Tooltip("In the editor only: run as if on another platform, to check its input and settings without making a build.")]
        [SerializeField] private bool _simulatePlatform;
        [SerializeField] private PlatformKind _simulatedPlatform = PlatformKind.Mobile;

        protected override void Configure(IContainerBuilder builder)
        {
            // Without a handler the container drops exceptions thrown by async entry points,
            // and a failed boot would look like a blank screen with a clean console.
            builder.RegisterEntryPointExceptionHandler(Debug.LogException);

            // The only place that looks at what the game is running on. Everything else asks IPlatform.
            builder.RegisterInstance(CurrentPlatform());
            builder.RegisterInstance(new UnityScreenMetrics()).As<IScreenMetrics, ISafeArea>();
            builder.RegisterInstance(_platformSettings);
            builder.RegisterEntryPoint<PlatformConfigurator>();

            builder.Register<MessageBus>(Lifetime.Singleton).As<IMessageBus, IPublisher, ISubscriber>();
            builder.RegisterInstance<ISaveStore>(new PlayerPrefsSaveStore(SavePrefix));

            builder.Register<IRunSeedSource, ClockSeedSource>(Lifetime.Singleton);
            builder.Register<GameFlow>(Lifetime.Singleton).AsSelf().As<IGameFlow>();

            builder.RegisterEntryPoint<BootSequence>();
        }

        private IPlatform CurrentPlatform()
        {
            if (Application.isEditor && _simulatePlatform) return new FixedPlatform(_simulatedPlatform);
            return new RuntimePlatformService();
        }

        private void OnValidate()
        {
            if (_platformSettings == null) Debug.LogError($"{nameof(AppLifetimeScope)} on '{name}' has no platform settings.", this);
        }
    }
}
