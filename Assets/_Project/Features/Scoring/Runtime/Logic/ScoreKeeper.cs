using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.Scoring.Data;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Scoring;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Scoring.Logic
{
    /// <summary>
    /// Score is distance plus pickups. Pickups collected in quick succession build a multiplier,
    /// which is what rewards taking the risky line instead of the safe lane.
    /// </summary>
    internal sealed class ScoreKeeper : ISimulationSystem, IDisposable
    {
        private const int BaseMultiplier = 1;

        private readonly IRunProgress _progress;
        private readonly ScoringRules _rules;
        private readonly IPublisher _publisher;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private int _pickupPoints;
        private float _sinceLastPickup;
        private bool _comboAlive;

        public ScoreKeeper(IRunProgress progress, ScoringRules rules, IPublisher publisher, ISubscriber subscriber)
        {
            _progress = Guard.NotNull(progress, nameof(progress));
            _rules = Guard.NotNull(rules, nameof(rules));
            _publisher = Guard.NotNull(publisher, nameof(publisher));
            Guard.NotNull(subscriber, nameof(subscriber));

            subscriber.Subscribe<RunStarted>(OnRunStarted).AddTo(_subscriptions);
            subscriber.Subscribe<PickupCollected>(OnPickupCollected).AddTo(_subscriptions);

            Multiplier = BaseMultiplier;
        }

        public int Score { get; private set; }

        public int Multiplier { get; private set; }

        public void Tick(float deltaTime)
        {
            int multiplier = Multiplier;
            if (_comboAlive)
            {
                _sinceLastPickup += deltaTime;
                if (_sinceLastPickup > _rules.ComboWindowSeconds)
                {
                    _comboAlive = false;
                    multiplier = BaseMultiplier;
                }
            }

            Publish(CurrentScore(), multiplier);
        }

        public void Dispose() => _subscriptions.Dispose();

        private void OnRunStarted(RunStarted message)
        {
            _pickupPoints = 0;
            _sinceLastPickup = 0f;
            _comboAlive = false;

            // Listeners need the reset even if the last run also ended on zero.
            Publish(0, BaseMultiplier, force: true);
        }

        private void OnPickupCollected(PickupCollected message)
        {
            int multiplier = _comboAlive ? Mathf.Min(Multiplier + 1, _rules.MaxMultiplier) : BaseMultiplier;
            _comboAlive = true;
            _sinceLastPickup = 0f;
            _pickupPoints += message.Value * multiplier;

            Publish(CurrentScore(), multiplier);
        }

        private int CurrentScore() => Mathf.FloorToInt(_progress.Distance * _rules.PointsPerMetre) + _pickupPoints;

        // The HUD redraws on this message, so it only goes out when a visible number changed.
        private void Publish(int score, int multiplier, bool force = false)
        {
            if (!force && score == Score && multiplier == Multiplier) return;

            Score = score;
            Multiplier = multiplier;
            _publisher.Publish(new ScoreChanged(score, multiplier));
        }
    }
}
