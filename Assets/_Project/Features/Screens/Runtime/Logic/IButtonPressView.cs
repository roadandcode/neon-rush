using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IButtonPressView
    {
        /// <summary>Raised for every on-screen button, whichever screen it is on.</summary>
        event Action ButtonPressed;
    }
}
