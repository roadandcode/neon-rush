using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Data
{
    /// <summary>Something the runner must not touch. It stays on the track after a hit.</summary>
    [CreateAssetMenu(menuName = "Neon Rush/Track/Hazard", fileName = "Hazard")]
    internal sealed class HazardDefinition : TrackEntityDefinition
    {
        public override bool OnTouched(IPublisher publisher)
        {
            publisher.Publish(new HazardHit());
            return false;
        }
    }
}
