using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Pace.Data;
using RoadAndCode.NeonRush.Pace.Logic;
using RoadAndCode.NeonRush.Shared.Run;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Pace.Composition
{
    /// <summary>Registers the run clock. Install this first: everything else reads its values.</summary>
    public sealed class PaceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private DifficultyCurveAsset _curve;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance<IDifficultyCurve>(_curve);
            builder.Register<RunProgress>(Lifetime.Singleton).As<IRunProgress, ISimulationSystem>();
        }

        private void OnValidate()
        {
            if (_curve == null) Debug.LogError($"{nameof(PaceInstaller)} on '{name}' has no difficulty curve.", this);
        }
    }
}
