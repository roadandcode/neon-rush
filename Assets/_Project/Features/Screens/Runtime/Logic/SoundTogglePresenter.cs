using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Sound;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>The sound on/off control. The setting belongs to the sound feature; this shows it and asks for it to change.</summary>
    internal sealed class SoundTogglePresenter : IStartable, IDisposable
    {
        private readonly ISoundToggleView _view;
        private readonly ISoundSettings _settings;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        public SoundTogglePresenter(ISoundToggleView view, ISoundSettings settings, ISubscriber subscriber)
        {
            _view = Guard.NotNull(view, nameof(view));
            _settings = Guard.NotNull(settings, nameof(settings));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            _view.SetSoundOn(_settings.SoundOn);
            _view.Pressed += OnPressed;
            _subscription = _subscriber.Subscribe<SoundSettingChanged>(OnSettingChanged);
        }

        public void Dispose()
        {
            _view.Pressed -= OnPressed;
            _subscription?.Dispose();
        }

        private void OnPressed() => _settings.SetSoundOn(!_settings.SoundOn);

        // The view follows the setting, not the press: whatever changes it, the control stays true.
        private void OnSettingChanged(SoundSettingChanged message) => _view.SetSoundOn(message.SoundOn);
    }
}
