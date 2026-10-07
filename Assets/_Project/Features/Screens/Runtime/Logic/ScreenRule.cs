using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>One row of the screen table: this screen is up during these phases.</summary>
    internal sealed class ScreenRule
    {
        private readonly GamePhase[] _shownIn;

        public ScreenRule(IScreen screen, params GamePhase[] shownIn)
        {
            Screen = Guard.NotNull(screen, nameof(screen));
            _shownIn = Guard.NotNull(shownIn, nameof(shownIn));
        }

        public IScreen Screen { get; }

        public bool IsShownIn(GamePhase phase) => Array.IndexOf(_shownIn, phase) >= 0;
    }
}
