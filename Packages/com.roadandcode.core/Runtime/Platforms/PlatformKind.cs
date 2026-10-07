namespace RoadAndCode.Core.Platforms
{
    /// <summary>
    /// The platform families a game is tuned for. Deliberately coarse: what differs between them
    /// is input, screen and performance budget, not the operating system.
    /// </summary>
    public enum PlatformKind
    {
        Desktop,
        Mobile,
        Web,
    }
}
