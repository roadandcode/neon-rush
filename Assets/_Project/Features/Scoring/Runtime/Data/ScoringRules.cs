using System;
using UnityEngine;

namespace RoadAndCode.NeonRush.Scoring.Data
{
    [Serializable]
    internal sealed class ScoringRules
    {
        [SerializeField, Min(0f)] private float _pointsPerMetre = 1f;

        [Tooltip("Collecting another pickup within this many seconds raises the multiplier.")]
        [SerializeField, Min(0.1f)] private float _comboWindowSeconds = 2f;

        [SerializeField, Min(1)] private int _maxMultiplier = 5;

        public float PointsPerMetre => _pointsPerMetre;
        public float ComboWindowSeconds => _comboWindowSeconds;
        public int MaxMultiplier => _maxMultiplier;
    }
}
