namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// The controls on a screen, in the order a player would step through them. This is what
    /// lets keys and a gamepad use a screen: highlight one control, move to the next, press it.
    /// </summary>
    internal interface IControlList
    {
        int ControlCount { get; }

        /// <summary>Highlights one control and clears the rest. <see cref="ControlFocus.None"/> clears them all.</summary>
        void SetFocus(int index);

        /// <summary>Does exactly what a pointer press on that control does.</summary>
        void Press(int index);
    }
}
