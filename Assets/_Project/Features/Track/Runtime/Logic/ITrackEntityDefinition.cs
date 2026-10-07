using RoadAndCode.Core.Messaging;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>
    /// One kind of thing that can sit on the track. The simulation only knows this interface,
    /// so a new kind of hazard or pickup is a new asset type, not a change to the simulation.
    /// </summary>
    internal interface ITrackEntityDefinition
    {
        /// <summary>The space this entity occupies when placed at the given track position.</summary>
        Bounds BoundsAt(float x, float z);

        /// <summary>Called once, when the runner first touches it. Returns true if it should disappear.</summary>
        bool OnTouched(IPublisher publisher);
    }
}
