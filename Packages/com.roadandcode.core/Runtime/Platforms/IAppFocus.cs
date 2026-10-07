using System;

namespace RoadAndCode.Core.Platforms
{
    /// <summary>
    /// Whether the player is looking at the game: the window has focus, the browser tab is in
    /// front, the app has not been sent to the background by a call or the home button.
    /// </summary>
    public interface IAppFocus
    {
        bool HasFocus { get; }

        /// <summary>Raised with the new state each time focus is gained or lost.</summary>
        event Action<bool> Changed;
    }
}
