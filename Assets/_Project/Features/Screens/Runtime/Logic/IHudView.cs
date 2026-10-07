using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IHudView : IScreen
    {
        event Action PausePressed;

        void SetScore(int score);

        /// <param name="comboActive">True while the multiplier is above its resting value, so the view can call attention to it.</param>
        void SetMultiplier(int multiplier, bool comboActive);
    }
}
