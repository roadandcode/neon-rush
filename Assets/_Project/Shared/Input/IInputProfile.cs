using System.Collections.Generic;

namespace RoadAndCode.NeonRush.Shared.Input
{
    /// <summary>
    /// Which device families are in use on the current platform. Features read this to decide
    /// which input sources to create and which prompts to show. Nothing reads the platform itself.
    /// </summary>
    public interface IInputProfile
    {
        /// <summary>Active control scheme names, from <see cref="InputNames.Schemes"/>.</summary>
        IReadOnlyList<string> ControlSchemes { get; }

        bool Uses(string controlScheme);
    }
}
