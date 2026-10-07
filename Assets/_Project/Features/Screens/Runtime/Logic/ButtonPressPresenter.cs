using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Screens;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Tells the rest of the game that a button was pressed. The screens have no idea what, if
    /// anything, that is for; today it is a click sound.
    /// </summary>
    internal sealed class ButtonPressPresenter : IStartable, IDisposable
    {
        private readonly IButtonPressView _view;
        private readonly IPublisher _publisher;

        public ButtonPressPresenter(IButtonPressView view, IPublisher publisher)
        {
            _view = Guard.NotNull(view, nameof(view));
            _publisher = Guard.NotNull(publisher, nameof(publisher));
        }

        public void Start() => _view.ButtonPressed += OnButtonPressed;

        public void Dispose() => _view.ButtonPressed -= OnButtonPressed;

        private void OnButtonPressed() => _publisher.Publish(new ButtonPressed());
    }
}
