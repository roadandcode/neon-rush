using System.Collections.Generic;
using System.Threading;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>
    /// Start-up, second half: wait for everything the gameplay features need loaded, then open
    /// the menu. Nothing can be played, shown or pressed before this finishes, so no feature has
    /// to cope with content that is still on its way.
    /// </summary>
    internal sealed class GameplayBoot : IAsyncStartable
    {
        private readonly GameFlow _flow;
        private readonly IEnumerable<IStartupTask> _tasks;

        public GameplayBoot(GameFlow flow, IEnumerable<IStartupTask> tasks)
        {
            _flow = Guard.NotNull(flow, nameof(flow));
            _tasks = Guard.NotNull(tasks, nameof(tasks));
        }

        public async Awaitable StartAsync(CancellationToken cancellation)
        {
            foreach (IStartupTask task in _tasks)
            {
                await task.RunAsync(cancellation);
                cancellation.ThrowIfCancellationRequested();
            }

            _flow.FinishBoot();
        }
    }
}
