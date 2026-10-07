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
        // Long enough to see the hit, the sparks and the runner go down before the result covers them.
        private const float GameOverDelay = 0.85f;

        private const float NotWaiting = -1f;

        private readonly ScreenRule[] _rules;
        private readonly bool[] _up;
        private readonly float[] _waits;

        public ScreenTable(params ScreenRule[] rules)
        {
            _rules = Guard.NotNull(rules, nameof(rules));
            _up = new bool[rules.Length];
            _waits = new float[rules.Length];
            for (int i = 0; i < _waits.Length; i++) _waits[i] = NotWaiting;
        }

        public static ScreenTable ForGame(IScreen menu, IScreen hud, IScreen pause, IScreen gameOver)
        {
            return new ScreenTable(
                new ScreenRule(menu, GamePhase.Menu),

                // The HUD stays up behind the pause panel, so the score is still there to read.
                new ScreenRule(hud, GamePhase.Run, GamePhase.Paused),
                new ScreenRule(pause, GamePhase.Paused),
                new ScreenRule(gameOver, GameOverDelay, GamePhase.GameOver));
        }

        /// <summary>
        /// Takes down every screen that does not belong to the phase and puts up the ones that do,
        /// at once or after their rule's delay.
        /// </summary>
        public void Apply(GamePhase phase)
        {
            for (int i = 0; i < _rules.Length; i++)
            {
                ScreenRule rule = _rules[i];

                if (!rule.IsShownIn(phase))
                {
                    Set(i, false);
                    continue;
                }

                // Already up, or already on its way: a second phase that shows the same screen changes nothing.
                if (_up[i] || _waits[i] >= 0f) continue;

                if (rule.ShowAfter > 0f) _waits[i] = rule.ShowAfter;
                else Set(i, true);
            }
        }

        /// <summary>Counts down the screens that are being held back and puts up the ones whose time has come.</summary>
        public void Advance(float deltaTime)
        {
            for (int i = 0; i < _waits.Length; i++)
            {
                if (_waits[i] < 0f) continue;

                _waits[i] -= deltaTime;
                if (_waits[i] <= 0f) Set(i, true);
            }
        }

        private void Set(int index, bool up)
        {
            _up[index] = up;
            _waits[index] = NotWaiting;
            _rules[index].Screen.SetVisible(up);
        }
    }
}
