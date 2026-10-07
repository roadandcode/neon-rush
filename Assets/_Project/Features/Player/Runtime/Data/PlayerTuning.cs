using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Data
{
    /// <summary>Every number that shapes how the runner feels. Edited on the asset, read-only in code.</summary>
    [Serializable]
    internal sealed class PlayerTuning
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float _laneChangeSpeed = 18f;
        [SerializeField, Min(0.1f)] private float _jumpHeight = 1.7f;
        [SerializeField, Min(0.1f)] private float _jumpDuration = 0.62f;
        [SerializeField, Min(0.1f)] private float _slideDuration = 0.6f;

        [Tooltip("Fall speed in m/s when slide is pressed in the air.")]
        [SerializeField, Min(0.1f)] private float _diveSpeed = 22f;

        [Header("Body")]
        [SerializeField, Min(0.1f)] private float _standingHeight = 1.8f;
        [SerializeField, Min(0.1f)] private float _slidingHeight = 0.8f;
        [SerializeField, Min(0.1f)] private float _width = 0.9f;
        [SerializeField, Min(0.1f)] private float _depth = 0.9f;

        [Header("Feel")]
        [Tooltip("A jump or slide pressed slightly too early still fires if it becomes possible within this many seconds.")]
        [SerializeField, Min(0f)] private float _inputBufferTime = 0.12f;

        [Tooltip("Degrees of lean per metre still to travel sideways.")]
        [SerializeField] private float _leanPerMetre = 14f;
        [SerializeField, Min(0f)] private float _maxLean = 22f;

        [Header("Touch")]
        [Tooltip("How far a finger must travel to count as a swipe, as a share of the screen's shorter side.")]
        [SerializeField, Range(0.01f, 0.3f)] private float _swipeThreshold = 0.06f;

        public float LaneChangeSpeed => _laneChangeSpeed;
        public float JumpHeight => _jumpHeight;
        public float JumpDuration => _jumpDuration;
        public float SlideDuration => _slideDuration;
        public float DiveSpeed => _diveSpeed;
        public float StandingHeight => _standingHeight;
        public float SlidingHeight => _slidingHeight;
        public float Width => _width;
        public float Depth => _depth;
        public float InputBufferTime => _inputBufferTime;
        public float LeanPerMetre => _leanPerMetre;
        public float MaxLean => _maxLean;
        public float SwipeThreshold => _swipeThreshold;
    }
}
