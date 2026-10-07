using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Presentation
{
    /// <summary>
    /// The floor never moves. Its grid is drawn by the shader, and scrolling it by the distance
    /// travelled is what makes the track appear to rush past.
    /// </summary>
    public sealed class GroundView : MonoBehaviour
    {
        private static readonly int ScrollProperty = Shader.PropertyToID("_Scroll");

        [SerializeField] private Renderer _renderer;

        private MaterialPropertyBlock _block;

        public void SetDistance(float metres)
        {
            _block ??= new MaterialPropertyBlock();
            _block.SetFloat(ScrollProperty, metres);
            _renderer.SetPropertyBlock(_block);
        }

        private void OnValidate()
        {
            if (_renderer == null) Debug.LogError($"{nameof(GroundView)} on '{name}' has no renderer.", this);
        }
    }
}
