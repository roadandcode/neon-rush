using System;
using RoadAndCode.Core.Platforms;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface ISafeAreaView
    {
        /// <summary>The UI's own size changed, so insets that were applied in its units are out of date.</summary>
        event Action Resized;

        void SetInsets(ScreenInsets insets);
    }
}
