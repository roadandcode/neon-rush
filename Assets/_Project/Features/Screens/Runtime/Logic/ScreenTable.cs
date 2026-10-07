using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Which screens are up in which phase. <see cref="ForGame"/> is the only place that says so
    /// for this game; a new screen is a new rule there.
    /// </summary>
    internal sealed class ScreenTable
    {
        private readonly ScreenRule[] _rules;

        public ScreenTable(params ScreenRule[] rules)
        {
            _rules = Guard.NotNull(rules, nameof(rules));
        }

        public static ScreenTable ForGame(IScreen menu, IScreen hud, IScreen pause, IScreen gameOver)
        {
            return new ScreenTable(
                new ScreenRule(menu, GamePhase.Menu),

                // The HUD stays up behind the pause panel, so the score is still there to read.
                new ScreenRule(hud, GamePhase.Run, GamePhase.Paused),
                new ScreenRule(pause, GamePhase.Paused),
                new ScreenRule(gameOver, GamePhase.GameOver));
        }

        /// <summary>Shows every screen that belongs to the phase and hides every one that does not.</summary>
        public void Apply(GamePhase phase)
        {
            for (int i = 0; i < _rules.Length; i++)
            {
                _rules[i].Screen.SetVisible(_rules[i].IsShownIn(phase));
            }
        }
    }
}
