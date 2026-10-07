using RoadAndCode.NeonRush.Effects.Logic;
using RoadAndCode.NeonRush.Effects.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Effects.Composition
{
    /// <summary>Registers the particle effects for hits and pickups. Needs an IRunnerBody and the message bus from the scope.</summary>
    public sealed class EffectsInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private EffectsView _view;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(_view).As<IEffectsView>();
            builder.RegisterEntryPoint<EffectsDirector>();
        }

        private void OnValidate()
        {
            if (_view == null) Debug.LogError($"{nameof(EffectsInstaller)} on '{name}' has no effects view.", this);
        }
    }
}
