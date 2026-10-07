using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface ISoundToggleView
    {
        event Action Pressed;

        void SetSoundOn(bool on);
    }
}
