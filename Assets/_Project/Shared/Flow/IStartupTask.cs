using System.Threading;
using UnityEngine;

namespace RoadAndCode.NeonRush.Shared.Flow
{
    /// <summary>
    /// Work that has to finish before the menu opens, such as loading content. A feature
    /// registers one of these and the boot waits for it; the game stays in the Boot phase until
    /// every task is done.
    /// </summary>
    public interface IStartupTask
    {
        Awaitable RunAsync(CancellationToken cancellation);
    }
}
