using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>
    /// Composition root for everything that lives as long as the session: messaging, saves and
    /// the game flow. Lives in the Bootstrap scene. Gameplay is composed by
    /// <see cref="GameplayLifetimeScope"/>, as a child of this scope.
    /// </summary>
    public sealed class AppLifetimeScope : LifetimeScope
    {
        private const string SavePrefix = "neonrush.";

        protected override void Configure(IContainerBuilder builder)
        {
            // Without a handler the container drops exceptions thrown by async entry points,
            // and a failed boot would look like a blank screen with a clean console.
            builder.RegisterEntryPointExceptionHandler(Debug.LogException);

            builder.Register<MessageBus>(Lifetime.Singleton).As<IMessageBus, IPublisher, ISubscriber>();
            builder.RegisterInstance<ISaveStore>(new PlayerPrefsSaveStore(SavePrefix));

            builder.Register<IRunSeedSource, ClockSeedSource>(Lifetime.Singleton);
            builder.Register<GameFlow>(Lifetime.Singleton).AsSelf().As<IGameFlow>();

            builder.RegisterEntryPoint<BootSequence>();
        }
    }
}
