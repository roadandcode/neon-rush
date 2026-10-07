using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IMenuView : IScreen, IControlList
    {
        event Action PlayPressed;

        void SetBest(int best);
    }
}
