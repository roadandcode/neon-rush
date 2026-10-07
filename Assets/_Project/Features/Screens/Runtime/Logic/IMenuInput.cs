using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Menu intent from keys and gamepads, with the devices left behind: step to another control,
    /// or press the current one.
    /// </summary>
    internal interface IMenuInput
    {
        /// <summary>-1 for the previous control, +1 for the next.</summary>
        event Action<int> Moved;

        event Action Submitted;

        void SetEnabled(bool enabled);
    }
}
