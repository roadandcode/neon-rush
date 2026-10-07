using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Scoring.Data;
using RoadAndCode.NeonRush.Scoring.Logic;
using RoadAndCode.NeonRush.Shared.Scoring;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Scoring.Composition
{
    /// <summary>
    /// Registers scoring. Needs an IRunProgress and an ISaveStore from the scope. Install it after
    /// Track, so a pickup collected this tick is in this tick's score.
    /// </summary>
    public sealed class ScoringInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private ScoringRulesAsset _rules;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_rules.Rules);
            builder.Register<ScoreKeeper>(Lifetime.Singleton).AsSelf().As<ISimulationSystem>();

            // Nothing depends on the recorder, so the container would never create it. Building it
            // when the scope is ready is what puts its subscription in place.
            builder.Register<BestScoreRecorder>(Lifetime.Singleton).AsSelf().As<IBestScore>();
            builder.RegisterBuildCallback(container => container.Resolve<BestScoreRecorder>());
        }

        private void OnValidate()
        {
            if (_rules == null) Debug.LogError($"{nameof(ScoringInstaller)} on '{name}' has no rules asset.", this);
        }
    }
}
