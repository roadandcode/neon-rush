using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IPauseView : IScreen
    {
        event Action ResumePressed;

        event Action QuitPressed;
    }
}
