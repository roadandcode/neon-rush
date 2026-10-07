using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Player.Input
{
    /// <summary>
    /// Builds the runner's input for the current platform from its input profile: one source per
    /// kind of input the profile enables. A platform without touch never creates a swipe source.
    /// </summary>
    internal static class PlayerInputFactory
    {
        public static IPlayerActionSource Create(
            IInputProfile profile, InputActionAsset actions, IScreenMetrics screen, PlayerTuning tuning)
        {
            Guard.NotNull(profile, nameof(profile));

            var sources = new List<IPlayerActionSource>(2);

            if (profile.Uses(InputNames.Schemes.Keyboard) || profile.Uses(InputNames.Schemes.Gamepad))
            {
                sources.Add(new ButtonActionSource(actions));
            }

            if (profile.Uses(InputNames.Schemes.Pointer))
            {
                sources.Add(new SwipeActionSource(actions, screen, tuning));
            }

            Guard.Require(sources.Count > 0, "The input profile enables no control scheme the runner can use.");
            return sources.Count == 1 ? sources[0] : new CompositeActionSource(sources.ToArray());
        }
    }
}
