using UnityEngine;

namespace RoadAndCode.NeonRush.Scoring.Data
{
    [CreateAssetMenu(menuName = "Neon Rush/Scoring Rules", fileName = "ScoringRules")]
    internal sealed class ScoringRulesAsset : ScriptableObject
    {
        [SerializeField] private ScoringRules _rules = new ScoringRules();

        public ScoringRules Rules => _rules;
    }
}
