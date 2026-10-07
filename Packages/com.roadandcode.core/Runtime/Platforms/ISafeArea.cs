namespace RoadAndCode.Core.Platforms
{
    /// <summary>The part of the screen that is not under a notch, a camera cut-out or rounded corners.</summary>
    public interface ISafeArea
    {
        ScreenInsets Insets { get; }
    }
}
