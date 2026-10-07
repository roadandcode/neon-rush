using System;

namespace RoadAndCode.Core.Messaging
{
    /// <summary>Sends "something happened" notifications. Messages are structs so publishing never allocates.</summary>
    public interface IPublisher
    {
        void Publish<T>(in T message) where T : struct;
    }

    /// <summary>Receives messages of one type until the returned handle is disposed.</summary>
    public interface ISubscriber
    {
        IDisposable Subscribe<T>(Action<T> handler) where T : struct;
    }

    /// <summary>
    /// Both halves together. Most classes should ask for <see cref="IPublisher"/> or
    /// <see cref="ISubscriber"/> only, whichever they actually use.
    /// </summary>
    public interface IMessageBus : IPublisher, ISubscriber
    {
    }
}
