namespace RoadAndCode.Core.Platforms
{
    /// <summary>
    /// What the game is running on. This is the one place that knows, so nothing else has to
    /// check <c>Application.platform</c> or compile differently per platform.
    /// </summary>
    public interface IPlatform
    {
        PlatformKind Kind { get; }
    }
}
