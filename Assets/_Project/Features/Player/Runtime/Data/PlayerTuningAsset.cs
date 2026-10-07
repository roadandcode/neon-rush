using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Data
{
    [CreateAssetMenu(menuName = "Neon Rush/Player Tuning", fileName = "PlayerTuning")]
    internal sealed class PlayerTuningAsset : ScriptableObject
    {
        [SerializeField] private PlayerTuning _tuning = new PlayerTuning();

        public PlayerTuning Tuning => _tuning;
    }
}
