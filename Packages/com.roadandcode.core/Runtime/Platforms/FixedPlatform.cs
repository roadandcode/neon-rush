namespace RoadAndCode.Core.Platforms
{
    /// <summary>A platform chosen by hand: for tests, and for simulating another platform in the editor.</summary>
    public sealed class FixedPlatform : IPlatform
    {
        public FixedPlatform(PlatformKind kind)
        {
            Kind = kind;
        }

        public PlatformKind Kind { get; }
    }
}
