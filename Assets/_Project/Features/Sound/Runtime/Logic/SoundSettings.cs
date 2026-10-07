using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.NeonRush.Shared.Sound;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Sound.Logic
{
    /// <summary>Whether the player wants sound. Remembered between sessions; on unless they have said otherwise.</summary>
    internal sealed class SoundSettings : ISoundSettings, IStartable
    {
        private const string SaveKey = "sound";

        private readonly ISaveStore _store;
        private readonly IMasterVolume _volume;
        private readonly IPublisher _publisher;

        public SoundSettings(ISaveStore store, IMasterVolume volume, IPublisher publisher)
        {
            _store = Guard.NotNull(store, nameof(store));
            _volume = Guard.NotNull(volume, nameof(volume));
            _publisher = Guard.NotNull(publisher, nameof(publisher));

            SoundOn = !_store.TryLoad(SaveKey, out SoundRecord record) || record.SoundOn;
        }

        public bool SoundOn { get; private set; }

        public void Start() => _volume.SetMuted(!SoundOn);

        public void SetSoundOn(bool on)
        {
            if (on == SoundOn) return;

            SoundOn = on;
            _store.Save(SaveKey, new SoundRecord(on));
            _volume.SetMuted(!on);
            _publisher.Publish(new SoundSettingChanged(on));
        }

        [Serializable]
        private sealed class SoundRecord
        {
            [SerializeField] private bool _soundOn;

            public SoundRecord(bool soundOn)
            {
                _soundOn = soundOn;
            }

            public bool SoundOn => _soundOn;
        }
    }
}
