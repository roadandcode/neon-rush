using System;
using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;

namespace RoadAndCode.Core.Lifetime
{
    /// <summary>
    /// Collects subscriptions so an owner can release all of them with one <see cref="Dispose"/>.
    /// </summary>
    public sealed class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _items = new List<IDisposable>();
        private bool _disposed;

        public void Add(IDisposable item)
        {
            Guard.NotNull(item, nameof(item));

            // Adding to an owner that is already gone must not leak the subscription.
            if (_disposed)
            {
                item.Dispose();
                return;
            }

            _items.Add(item);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            for (int i = _items.Count - 1; i >= 0; i--) _items[i].Dispose();
            _items.Clear();
        }
    }

    public static class DisposableExtensions
    {
        public static T AddTo<T>(this T item, CompositeDisposable owner) where T : IDisposable
        {
            owner.Add(item);
            return item;
        }
    }
}
