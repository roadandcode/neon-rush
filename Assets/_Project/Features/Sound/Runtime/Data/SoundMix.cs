using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Sound.Data
{
    /// <summary>The numbers behind how the game sounds: levels per phase and how sounds respond to play.</summary>
    [Serializable]
    internal sealed class SoundMix
    {
        [Header("Music")]
        [Tooltip("Seconds to fade between music levels when the phase changes.")]
        [SerializeField, Min(0.01f)] private float _musicFadeSeconds = 0.8f;

        [Tooltip("Level of the run music while paused, as a share of its normal level.")]
        [SerializeField, Range(0f, 1f)] private float _pausedMusicLevel = 0.3f;

        [Tooltip("Level of the run music on the game-over screen.")]
        [SerializeField, Range(0f, 1f)] private float _gameOverMusicLevel = 0.45f;

        [Header("Pickups")]
        [Tooltip("Each step of the combo multiplier raises the pickup sound by this many semitones, so a streak climbs.")]
        [SerializeField, Min(0f)] private float _pickupSemitonesPerCombo = 2f;

        [Tooltip("The pitch stops rising after this many steps.")]
        [SerializeField, Min(0)] private int _pickupMaxSteps = 6;

        [Header("Results")]
        [Tooltip("Seconds after the crash before the new-best sound, so the two don't land on top of each other.")]
        [SerializeField, Min(0f)] private float _newBestDelay = 0.55f;

        public float MusicFadeSeconds => _musicFadeSeconds;
        public float PausedMusicLevel => _pausedMusicLevel;
        public float GameOverMusicLevel => _gameOverMusicLevel;
        public float PickupSemitonesPerCombo => _pickupSemitonesPerCombo;
        public int PickupMaxSteps => _pickupMaxSteps;
        public float NewBestDelay => _newBestDelay;
    }
}
