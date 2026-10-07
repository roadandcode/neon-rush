using UnityEngine;

namespace RoadAndCode.NeonRush.Shared.Track
{
    [CreateAssetMenu(menuName = "Neon Rush/Lane Layout", fileName = "LaneLayout")]
    public sealed class LaneLayoutAsset : ScriptableObject
    {
        [SerializeField, Min(1)] private int _laneCount = 3;
        [SerializeField, Min(0.1f)] private float _laneWidth = 2.5f;

        public ILaneLayout CreateGrid() => new LaneGrid(_laneCount, _laneWidth);
    }
}
