using System;
using RoadAndCode.Core.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace RoadAndCode.Core.Pooling
{
    /// <summary>
    /// Reuses instances of one prefab. Released instances are deactivated and parked under
    /// <c>parent</c>, so nothing spawned during play costs an Instantiate after warm-up.
    /// </summary>
    public sealed class ComponentPool<T> : IDisposable where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public ComponentPool(T prefab, Transform parent, int prewarm = 0, int maxSize = 256)
        {
            Guard.NotNull(prefab, nameof(prefab));

            _pool = new ObjectPool<T>(
                createFunc: () => Object.Instantiate(prefab, parent),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item => item.gameObject.SetActive(false),
                actionOnDestroy: item =>
                {
                    if (item != null) Object.Destroy(item.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: Math.Max(prewarm, 8),
                maxSize: maxSize);

            Prewarm(prewarm);
        }

        public int CountInactive => _pool.CountInactive;

        public T Get() => _pool.Get();

        public void Release(T item) => _pool.Release(item);

        public void Dispose() => _pool.Dispose();

        private void Prewarm(int count)
        {
            if (count <= 0) return;

            var warmed = new T[count];
            for (int i = 0; i < count; i++) warmed[i] = _pool.Get();
            for (int i = 0; i < count; i++) _pool.Release(warmed[i]);
        }
    }
}
