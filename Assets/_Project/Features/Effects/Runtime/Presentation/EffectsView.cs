using RoadAndCode.NeonRush.Effects.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Effects.Presentation
{
    /// <summary>
    /// Two particle systems that are always in the scene and only ever asked to emit. Their
    /// particles live in world space, so moving a system to the next burst leaves the last
    /// one where it was, and nothing is spawned or destroyed while the game plays.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class EffectsView : MonoBehaviour, IEffectsView
    {
        [SerializeField] private ParticleSystem _impact;
        [SerializeField, Min(1)] private int _impactParticles = 48;

        [SerializeField] private ParticleSystem _pickup;
        [SerializeField, Min(1)] private int _pickupParticles = 14;

        public void PlayImpact(Vector3 position) => Burst(_impact, position, _impactParticles);

        public void PlayPickup(Vector3 position) => Burst(_pickup, position, _pickupParticles);

        private static void Burst(ParticleSystem particles, Vector3 position, int count)
        {
            particles.transform.position = position;
            particles.Emit(count);
        }

        private void OnValidate()
        {
            if (_impact == null) Debug.LogError($"{nameof(EffectsView)} on '{name}' has no impact particle system.", this);
            if (_pickup == null) Debug.LogError($"{nameof(EffectsView)} on '{name}' has no pickup particle system.", this);
        }
    }
}
