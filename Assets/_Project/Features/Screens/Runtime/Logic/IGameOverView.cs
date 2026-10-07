using System;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IGameOverView : IScreen
    {
        event Action RetryPressed;

        event Action MenuPressed;

        void ShowResult(int score, int best, bool isNewBest);
    }
}
