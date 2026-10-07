using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Data
{
    /// <summary>Something worth collecting. It disappears when touched.</summary>
    [CreateAssetMenu(menuName = "Neon Rush/Track/Pickup", fileName = "Pickup")]
    internal sealed class PickupDefinition : TrackEntityDefinition
    {
        [SerializeField, Min(0)] private int _value = 10;

        public override bool OnTouched(IPublisher publisher)
        {
            publisher.Publish(new PickupCollected(_value));
            return true;
        }
    }
}
