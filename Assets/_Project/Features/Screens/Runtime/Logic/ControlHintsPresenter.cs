using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Input;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Decides which control hints the player sees. The answer comes from the platform's input
    /// profile, so a phone is told to swipe and a desktop is told which keys to press without the
    /// UI ever asking what it is running on.
    /// </summary>
    internal sealed class ControlHintsPresenter : IStartable
    {
        private readonly IControlHintsView _view;
        private readonly IInputProfile _profile;

        public ControlHintsPresenter(IControlHintsView view, IInputProfile profile)
        {
            _view = Guard.NotNull(view, nameof(view));
            _profile = Guard.NotNull(profile, nameof(profile));
        }

        public void Start()
        {
            for (int i = 0; i < _profile.ControlSchemes.Count; i++)
            {
                _view.ShowHintsFor(_profile.ControlSchemes[i]);
            }
        }
    }
}
