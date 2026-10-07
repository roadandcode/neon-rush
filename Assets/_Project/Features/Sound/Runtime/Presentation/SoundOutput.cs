using RoadAndCode.NeonRush.Sound.Data;
using RoadAndCode.NeonRush.Sound.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Sound.Presentation
{
    /// <summary>
    /// The only class that touches Unity audio. One-shot sounds take turns on a fixed set of
    /// audio sources, so nothing is created while the game plays; the two music loops each have
    /// their own.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class SoundOutput : MonoBehaviour, ISoundPlayer, IMusicPlayer, IMasterVolume
    {
        // Long enough for both loops to be queued before the audio clock reaches the start time.
        private const double MusicLeadSeconds = 0.1;

        [SerializeField] private SoundBankAsset _bank;

        [Tooltip("One-shot sounds take turns on these. With all of them busy, the oldest is cut off.")]
        [SerializeField] private AudioSource[] _voices;

        [SerializeField] private AudioSource _menuMusic;
        [SerializeField] private AudioSource _runMusic;

        private int _nextVoice;
        private bool _musicStarted;

        public SoundBankAsset Bank => _bank;

        public void Play(SoundCue cue, float pitch = 1f, float delay = 0f)
        {
            if (!_bank.TryFind(cue, out SoundBankAsset.Entry sound)) return;

            AudioSource voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;

            voice.clip = sound.Clip;
            voice.volume = sound.Volume;
            voice.pitch = pitch;

            if (delay > 0f) voice.PlayDelayed(delay);
            else voice.Play();
        }

        public void SetLevels(float menu, float run)
        {
            StartMusicOnce();
            _menuMusic.volume = menu * _bank.MusicVolume;
            _runMusic.volume = run * _bank.MusicVolume;
        }

        public void SetMuted(bool muted) => AudioListener.volume = muted ? 0f : 1f;

        // Both loops are scheduled for the same moment on the audio clock. Started with Play() they
        // would begin a few milliseconds apart, and two copies of the same chords that far out of
        // step sound like a mistake.
        private void StartMusicOnce()
        {
            if (_musicStarted) return;
            _musicStarted = true;

            double startAt = AudioSettings.dspTime + MusicLeadSeconds;
            Queue(_menuMusic, _bank.MenuMusic, startAt);
            Queue(_runMusic, _bank.RunMusic, startAt);
        }

        private static void Queue(AudioSource source, AudioClip clip, double startAt)
        {
            source.clip = clip;
            source.loop = true;
            source.volume = 0f;
            source.PlayScheduled(startAt);
        }

        private void OnValidate()
        {
            if (_bank == null) Debug.LogError($"{nameof(SoundOutput)} on '{name}' has no sound bank.", this);
            if (_voices == null || _voices.Length == 0) Debug.LogError($"{nameof(SoundOutput)} on '{name}' has no voices.", this);
            if (_menuMusic == null || _runMusic == null) Debug.LogError($"{nameof(SoundOutput)} on '{name}' is missing a music source.", this);
        }
    }
}
