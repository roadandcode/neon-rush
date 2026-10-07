using System;
using System.Collections.Generic;
using System.Threading;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Data;
using RoadAndCode.NeonRush.Track.Logic;
using RoadAndCode.NeonRush.Track.Presentation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoadAndCode.NeonRush.Track.Infrastructure
{
    /// <summary>
    /// Loads the view prefab of every entity the track's patterns can place, once, before the
    /// menu opens, and hands them out from memory after that. Spawning during a run therefore
    /// never waits on a load, and the handles are given back when the scope goes away.
    ///
    /// This is the only class in the feature that knows Addressables exists.
    /// </summary>
    internal sealed class AddressableTrackViews : ITrackViewCatalog, IStartupTask, IDisposable
    {
        private readonly IReadOnlyList<ITrackPattern> _patterns;
        private readonly ILaneLayout _lanes;
        private readonly Dictionary<TrackEntityDefinition, TrackEntityView> _prefabs = new Dictionary<TrackEntityDefinition, TrackEntityView>();
        private readonly List<AsyncOperationHandle<GameObject>> _handles = new List<AsyncOperationHandle<GameObject>>();

        public AddressableTrackViews(IReadOnlyList<ITrackPattern> patterns, ILaneLayout lanes)
        {
            _patterns = Guard.NotNull(patterns, nameof(patterns));
            _lanes = Guard.NotNull(lanes, nameof(lanes));
        }

        public async Awaitable RunAsync(CancellationToken cancellation)
        {
            // Start every load before waiting on any of them, so they overlap.
            var definitions = new List<TrackEntityDefinition>(DefinitionsInUse());
            foreach (TrackEntityDefinition definition in definitions)
            {
                _handles.Add(Addressables.LoadAssetAsync<GameObject>(definition.View));
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                AsyncOperationHandle<GameObject> handle = _handles[i];
                await Completion(handle);
                cancellation.ThrowIfCancellationRequested();

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new InvalidOperationException($"Could not load the view for '{definitions[i].name}'.", handle.OperationException);
                }

                if (!handle.Result.TryGetComponent(out TrackEntityView view))
                {
                    throw new InvalidOperationException($"The view for '{definitions[i].name}' has no {nameof(TrackEntityView)} on its root.");
                }

                _prefabs.Add(definitions[i], view);
            }
        }

        public TrackEntityView PrefabFor(TrackEntityDefinition definition)
        {
            return _prefabs.TryGetValue(definition, out TrackEntityView prefab) ? prefab : null;
        }

        public void Dispose()
        {
            foreach (AsyncOperationHandle<GameObject> handle in _handles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }

            _handles.Clear();
            _prefabs.Clear();
        }

        // Addressables' own Task is not available on WebGL, so completion is bridged by callback.
        private static Awaitable Completion(AsyncOperationHandle<GameObject> handle)
        {
            var completion = new AwaitableCompletionSource();
            if (handle.IsDone) completion.SetResult();
            else handle.Completed += _ => completion.SetResult();
            return completion.Awaitable;
        }

        private IEnumerable<TrackEntityDefinition> DefinitionsInUse()
        {
            var seen = new HashSet<TrackEntityDefinition>();
            foreach (ITrackPattern pattern in _patterns)
            {
                for (int row = 0; row < pattern.RowCount; row++)
                {
                    for (int lane = 0; lane < _lanes.LaneCount; lane++)
                    {
                        if (pattern.CellAt(row, lane) is TrackEntityDefinition definition && seen.Add(definition)) yield return definition;
                    }
                }
            }
        }
    }
}
