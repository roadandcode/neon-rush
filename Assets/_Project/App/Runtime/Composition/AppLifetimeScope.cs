using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>
    /// Composition root for the whole session. This is the only place that decides which
    /// concrete class stands behind each interface. Feature installers are added here, and the
    /// order they register simulation systems in is the order those systems tick.
    /// </summary>
    public sealed class AppLifetimeScope : LifetimeScope
    {
        private const string SavePrefix = "neonrush.";

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MessageBus>(Lifetime.Singleton).As<IMessageBus, IPublisher, ISubscriber>();
            builder.RegisterInstance<ISaveStore>(new PlayerPrefsSaveStore(SavePrefix));

            builder.Register<SimulationLoop>(Lifetime.Singleton);
            builder.Register<IRunSeedSource, ClockSeedSource>(Lifetime.Singleton);
            builder.Register<GameFlow>(Lifetime.Singleton).AsSelf().As<IGameFlow>();

            builder.RegisterEntryPoint<AppEntryPoint>();
        }
    }
}
