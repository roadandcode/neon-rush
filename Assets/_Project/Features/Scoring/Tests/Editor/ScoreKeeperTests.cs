using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Scoring.Data;
using RoadAndCode.NeonRush.Scoring.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Scoring;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Scoring.Tests
{
    internal sealed class FixedProgress : IRunProgress
    {
        public float Elapsed { get; set; }

        public float Distance { get; set; }

        public float Speed { get; set; }

        public int Tier { get; set; }
    }

    public sealed class ScoreKeeperTests
    {
        private const float Step = 0.1f;
        private const int PickupValue = 10;

        private ScoringRules _rules;
        private FixedProgress _progress;
        private MessageBus _bus;
        private ScoreKeeper _keeper;
        private List<(int Score, int Multiplier)> _changes;

        [SetUp]
        public void SetUp()
        {
            _rules = new ScoringRules();
            _progress = new FixedProgress();
            _bus = new MessageBus();
            _keeper = new ScoreKeeper(_progress, _rules, _bus, _bus);
            _changes = new List<(int, int)>();
            _bus.Subscribe<ScoreChanged>(m => _changes.Add((m.Score, m.Multiplier)));
        }

        private void Collect() => _bus.Publish(new PickupCollected(PickupValue));

        [Test]
        public void Distance_IsWorthPointsPerMetre_RoundedDown()
        {
            _progress.Distance = 12.7f;

            _keeper.Tick(Step);

            Assert.That(_keeper.Score, Is.EqualTo((int)(12.7f * _rules.PointsPerMetre)));
        }

        [Test]
        public void ScoreChanged_IsOnlySentWhenAVisibleNumberChanges()
        {
            _progress.Distance = 5.2f;
            _keeper.Tick(Step);
            _progress.Distance = 5.9f;
            _keeper.Tick(Step);
            _progress.Distance = 6.1f;
            _keeper.Tick(Step);

            Assert.That(_changes, Is.EqualTo(new[] { (5, 1), (6, 1) }));
        }

        [Test]
        public void APickup_AddsItsValue()
        {
            Collect();

            Assert.That(_keeper.Score, Is.EqualTo(PickupValue));
            Assert.That(_keeper.Multiplier, Is.EqualTo(1));
        }

        [Test]
        public void PickupsInQuickSuccession_BuildTheMultiplier()
        {
            Collect();
            _keeper.Tick(Step);
            Collect();
            _keeper.Tick(Step);
            Collect();

            Assert.That(_keeper.Multiplier, Is.EqualTo(3));
            Assert.That(_keeper.Score, Is.EqualTo(PickupValue * (1 + 2 + 3)));
        }

        [Test]
        public void TheMultiplier_StopsAtTheMaximum()
        {
            for (int i = 0; i < _rules.MaxMultiplier + 3; i++) Collect();

            Assert.That(_keeper.Multiplier, Is.EqualTo(_rules.MaxMultiplier));
        }

        [Test]
        public void TheMultiplier_DropsBackOnceTheComboWindowPasses()
        {
            Collect();
            Collect();
            Assert.That(_keeper.Multiplier, Is.EqualTo(2));

            for (float t = 0f; t <= _rules.ComboWindowSeconds + Step; t += Step) _keeper.Tick(Step);

            Assert.That(_keeper.Multiplier, Is.EqualTo(1));
            Assert.That(_changes[_changes.Count - 1], Is.EqualTo((PickupValue * 3, 1)));
        }

        [Test]
        public void APickupAfterTheWindow_StartsANewCombo()
        {
            Collect();
            Collect();
            for (float t = 0f; t <= _rules.ComboWindowSeconds + Step; t += Step) _keeper.Tick(Step);

            Collect();

            Assert.That(_keeper.Multiplier, Is.EqualTo(1));
            Assert.That(_keeper.Score, Is.EqualTo(PickupValue * 4));
        }

        [Test]
        public void RunStarted_ResetsTheScore_AndAlwaysAnnouncesIt()
        {
            _bus.Publish(new RunStarted(seed: 1));
            Collect();
            Collect();
            _changes.Clear();

            _progress.Distance = 0f;
            _bus.Publish(new RunStarted(seed: 2));

            Assert.That(_keeper.Score, Is.Zero);
            Assert.That(_keeper.Multiplier, Is.EqualTo(1));
            Assert.That(_changes, Is.EqualTo(new[] { (0, 1) }));

            _changes.Clear();
            _bus.Publish(new RunStarted(seed: 3));
            Assert.That(_changes, Is.EqualTo(new[] { (0, 1) }), "A reset from zero to zero is still announced.");
        }

        [Test]
        public void ScoringARun_AllocatesNothing()
        {
            var bus = new MessageBus();
            var keeper = new ScoreKeeper(_progress, _rules, bus, bus);
            int announced = 0;
            bus.Subscribe<ScoreChanged>(_ => announced++);
            bus.Publish(new PickupCollected(PickupValue));
            keeper.Tick(Step);

            Assert.That(() =>
            {
                for (int i = 0; i < 300; i++)
                {
                    _progress.Distance += 0.5f;
                    if (i % 30 == 0) bus.Publish(new PickupCollected(PickupValue));
                    keeper.Tick(Step);
                }
            }, Is.Not.AllocatingGCMemory());
            Assert.That(announced, Is.GreaterThan(0));
        }
    }
}
