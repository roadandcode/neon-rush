using UnityEngine;

namespace RoadAndCode.NeonRush.Shared.Run
{
    /// <summary>The space the runner occupies right now, in track space (the runner sits at z = 0).</summary>
    public interface IRunnerBody
    {
        Bounds Bounds { get; }
    }
}
