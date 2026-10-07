using System;
using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;

namespace RoadAndCode.Core.Messaging
{
    /// <summary>
    /// Typed publish/subscribe for decoupled modules. Main thread only.
    /// Publishing is allocation-free; subscribing and unsubscribing allocate.
    /// </summary>
    public sealed class MessageBus : IMessageBus
    {
        private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public void Publish<T>(in T message) where T : struct
        {
            if (_channels.TryGetValue(typeof(T), out var channel))
            {
                ((Channel<T>)channel).Publish(message);
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : struct
        {
            Guard.NotNull(handler, nameof(handler));

            if (!_channels.TryGetValue(typeof(T), out var channel))
            {
                channel = new Channel<T>();
                _channels.Add(typeof(T), channel);
            }

            return ((Channel<T>)channel).Add(handler);
        }

        private sealed class Channel<T> where T : struct
        {
            // Copy-on-write: a publish in progress keeps iterating the array it started with,
            // so handlers can subscribe or unsubscribe from inside a callback.
            private Action<T>[] _handlers = Array.Empty<Action<T>>();

            public void Publish(in T message)
            {
                var handlers = _handlers;
                for (int i = 0; i < handlers.Length; i++) handlers[i](message);
            }

            public IDisposable Add(Action<T> handler)
            {
                var next = new Action<T>[_handlers.Length + 1];
                Array.Copy(_handlers, next, _handlers.Length);
                next[_handlers.Length] = handler;
                _handlers = next;
                return new Subscription(this, handler);
            }

            private void Remove(Action<T> handler)
            {
                int index = Array.IndexOf(_handlers, handler);
                if (index < 0) return;

                var next = new Action<T>[_handlers.Length - 1];
                Array.Copy(_handlers, 0, next, 0, index);
                Array.Copy(_handlers, index + 1, next, index, next.Length - index);
                _handlers = next;
            }

            private sealed class Subscription : IDisposable
            {
                private Channel<T> _channel;
                private readonly Action<T> _handler;

                public Subscription(Channel<T> channel, Action<T> handler)
                {
                    _channel = channel;
                    _handler = handler;
                }

                public void Dispose()
                {
                    _channel?.Remove(_handler);
                    _channel = null;
                }
            }
        }
    }
}
