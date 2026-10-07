using RoadAndCode.NeonRush.Pace.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Pace.Data
{
    [CreateAssetMenu(menuName = "Neon Rush/Difficulty Curve", fileName = "DifficultyCurve")]
    internal sealed class DifficultyCurveAsset : ScriptableObject, IDifficultyCurve
    {
        [SerializeField] private AnimationCurve _speedOverTime = AnimationCurve.EaseInOut(0f, 14f, 120f, 34f);

        [Tooltip("Seconds into the run at which each further tier begins, in ascending order.")]
        [SerializeField] private float[] _tierStartTimes = { 15f, 40f, 75f };

        public float SpeedAt(float elapsed) => _speedOverTime.Evaluate(elapsed);

        public int TierAt(float elapsed)
        {
            int tier = 0;
            while (tier < _tierStartTimes.Length && elapsed >= _tierStartTimes[tier]) tier++;
            return tier;
        }

        private void OnValidate()
        {
            for (int i = 1; i < _tierStartTimes.Length; i++)
            {
                if (_tierStartTimes[i] < _tierStartTimes[i - 1])
                {
                    Debug.LogError($"{name}: tier start times must be in ascending order.", this);
                    return;
                }
            }
        }
    }
}
