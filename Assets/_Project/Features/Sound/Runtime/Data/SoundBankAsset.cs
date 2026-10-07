using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Sound.Data
{
    /// <summary>
    /// The game's audio in one asset: which clip each cue plays, the two music loops, and the
    /// mix. Replacing a sound is dragging a different clip in here.
    /// </summary>
    [CreateAssetMenu(menuName = "Neon Rush/Sound Bank", fileName = "SoundBank")]
    internal sealed class SoundBankAsset : ScriptableObject
    {
        [Serializable]
        internal struct Entry
        {
            [SerializeField] private SoundCue _cue;
            [SerializeField] private AudioClip _clip;
            [SerializeField, Range(0f, 1f)] private float _volume;

            public SoundCue Cue => _cue;
            public AudioClip Clip => _clip;
            public float Volume => _volume;
        }

        [SerializeField] private Entry[] _sounds = Array.Empty<Entry>();

        [Header("Music")]
        [Tooltip("Plays on the title screen. Both loops must be the same length and tempo: they run in step and are faded between.")]
        [SerializeField] private AudioClip _menuMusic;

        [Tooltip("Plays during a run.")]
        [SerializeField] private AudioClip _runMusic;

        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;

        [SerializeField] private SoundMix _mix = new SoundMix();

        public AudioClip MenuMusic => _menuMusic;
        public AudioClip RunMusic => _runMusic;
        public float MusicVolume => _musicVolume;
        public SoundMix Mix => _mix;
        public int SoundCount => _sounds.Length;

        public Entry SoundAt(int index) => _sounds[index];

        public bool TryFind(SoundCue cue, out Entry sound)
        {
            for (int i = 0; i < _sounds.Length; i++)
            {
                if (_sounds[i].Cue != cue) continue;

                sound = _sounds[i];
                return true;
            }

            sound = default;
            return false;
        }

        private void OnValidate()
        {
            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                if (!TryFind(cue, out Entry sound) || sound.Clip == null) Debug.LogError($"{name}: no clip for the {cue} cue.", this);
            }

            if (_menuMusic == null || _runMusic == null) Debug.LogError($"{name}: a music loop is missing.", this);
        }
    }
}
