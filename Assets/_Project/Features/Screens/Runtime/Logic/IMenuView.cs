using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IMenuView : IScreen
    {
        event Action PlayPressed;

        void SetBest(int best);
    }
}
