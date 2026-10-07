using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IPauseView : IScreen, IControlList
    {
        event Action ResumePressed;

        event Action QuitPressed;
    }
}
