namespace RoadAndCode.NeonRush.Shared.Track
{
    /// <summary>The runner touched a hazard. What that costs is the player's decision, not the track's.</summary>
    public readonly struct HazardHit
    {
    }

    public readonly struct PickupCollected
    {
        public readonly int Value;

        public PickupCollected(int value)
        {
            Value = value;
        }
    }
}
