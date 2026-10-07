using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Randomness;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Data;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Track.Tests
{
    public sealed class TrackSimulationTests
    {
        private const float Tolerance = 1e-3f;

        private TrackSettings _settings;
        private LaneGrid _lanes;
        private TrackField _field;
        private FixedProgress _progress;
        private FixedBody _body;
        private MessageBus _bus;
        private List<string> _messages;

        [SetUp]
        public void SetUp()
        {
            _settings = new TrackSettings();
            _lanes = new LaneGrid(3, 2.5f);
            _field = new TrackField();
            _progress = new FixedProgress();
            _body = new FixedBody();
            _bus = new MessageBus();
            _messages = new List<string>();

            _bus.Subscribe<HazardHit>(_ => _messages.Add("hit"));
            _bus.Subscribe<PickupCollected>(m => _messages.Add("pickup:" + m.Value));
        }

        /// <summary>A simulation whose spawner never has anything due, so tests place entities by hand.</summary>
        private TrackSimulation QuietSimulation()
        {
            var empty = new FakePattern("empty-row", 0, 1f, new ITrackEntityDefinition[] { null, null, null });
            var spawner = new TrackSpawner(_field, new SinglePatternPicker(empty), _lanes, _settings);
            return new TrackSimulation(_field, spawner, _progress, _body, _bus, _bus, _settings, new SeededRandom(1));
        }

        [Test]
        public void Tick_BringsEverythingCloserBySpeedTimesDeltaTime()
        {
            var simulation = QuietSimulation();
            var entity = _field.Spawn(FakeDefinition.Hazard("block"), _lanes.CenterOf(2), 50f);
            _progress.Speed = 20f;

            simulation.Tick(0.25f);

            Assert.That(entity.Z, Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void EntitiesWellBehindTheRunner_AreRecycled()
        {
            var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("gone"), _lanes.CenterOf(2), -_settings.DespawnDistance + 0.5f);
            var ahead = _field.Spawn(FakeDefinition.Hazard("ahead"), _lanes.CenterOf(2), 30f);
            _progress.Speed = 10f;

            simulation.Tick(0.1f);

            Assert.That(_field.Entities, Is.EquivalentTo(new[] { ahead }));
        }

        [Test]
        public void TouchingAHazard_ReportsOneHit_HoweverLongTheOverlapLasts()
        {
            var simulation = QuietSimulation();
            var hazard = FakeDefinition.Hazard("block", height: 2f);
            _field.Spawn(hazard, x: 0f, z: 0f);

            simulation.Tick(0.016f);
            simulation.Tick(0.016f);
            simulation.Tick(0.016f);

            Assert.That(_messages, Is.EqualTo(new[] { "hit" }));
            Assert.That(hazard.Touches, Is.EqualTo(1));
            Assert.That(_field.Entities, Has.Count.EqualTo(1), "A hazard stays on the track after it is hit.");
        }

        [Test]
        public void TouchingAPickup_ReportsItsValue_AndRemovesIt()
        {
            var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Pickup("shard", 25), x: 0f, z: 0f);

            simulation.Tick(0.016f);

            Assert.That(_messages, Is.EqualTo(new[] { "pickup:25" }));
            Assert.That(_field.Entities, Is.Empty);
        }

        [Test]
        public void ARunnerInAnotherLane_TouchesNothing()
        {
            var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("block", height: 2f), _lanes.CenterOf(2), 0f);

            simulation.Tick(0.016f);

            Assert.That(_messages, Is.Empty);
        }

        [Test]
        public void ARunnerAboveALowHazard_ClearsIt()
        {
            var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("hurdle", height: 0.7f), 0f, 0f);
            _body.Bounds = new Bounds(new Vector3(0f, 1.0f + 0.9f, 0f), new Vector3(0.9f, 1.8f, 0.9f));

            simulation.Tick(0.016f);

            Assert.That(_messages, Is.Empty);
        }

        [Test]
        public void ALowRunnerUnderAnOverheadHazard_ClearsIt()
        {
            var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("gate", bottom: 1.1f, height: 1.4f), 0f, 0f);
            _body.Bounds = new Bounds(new Vector3(0f, 0.4f, 0f), new Vector3(0.9f, 0.8f, 0.9f));

            simulation.Tick(0.016f);

            Assert.That(_messages, Is.Empty);
        }

        [Test]
        public void AStandingRunnerUnderAnOverheadHazard_IsHit()
        {
            var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("gate", bottom: 1.1f, height: 1.4f), 0f, 0f);

            simulation.Tick(0.016f);

            Assert.That(_messages, Is.EqualTo(new[] { "hit" }));
        }

        [Test]
        public void ClearingTheStage_LeavesNothingOnTheTrack()
        {
            using var simulation = QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("left over"), _lanes.CenterOf(1), 4f);
            _field.Spawn(FakeDefinition.Hazard("further on"), _lanes.CenterOf(0), 40f);

            _bus.Publish(new StageCleared());

            Assert.That(_field.Entities, Is.Empty);
        }

        [Test]
        public void RunStarted_ClearsTheTrack()
        {
            QuietSimulation();
            _field.Spawn(FakeDefinition.Hazard("left-over"), 0f, 40f);

            _bus.Publish(new RunStarted(seed: 3));

            Assert.That(_field.Entities, Is.Empty);
        }

        [Test]
        public void TheSameSeed_LaysOutTheSameTrack()
        {
            var a = FakeDefinition.Hazard("a");
            var b = FakeDefinition.Hazard("b");
            var c = FakeDefinition.Hazard("c");
            var patterns = new ITrackPattern[]
            {
                new FakePattern("A", 0, 1f, new ITrackEntityDefinition[] { a, null, null }),
                new FakePattern("B", 0, 1f, new ITrackEntityDefinition[] { null, b, null }),
                new FakePattern("C", 0, 1f, new ITrackEntityDefinition[] { null, null, c }),
            };
            var random = new SeededRandom(1);
            var spawner = new TrackSpawner(_field, new WeightedPatternPicker(patterns, random), _lanes, _settings);
            using var simulation = new TrackSimulation(_field, spawner, _progress, _body, _bus, _bus, _settings, random);
            _body.Bounds = new Bounds(new Vector3(0f, 50f, 0f), Vector3.one);
            _progress.Speed = 30f;

            var spawned = new List<string>();
            _field.Added += entity => spawned.Add(entity.Definition.ToString());

            List<string> RunWithSeed(int seed)
            {
                spawned.Clear();
                _bus.Publish(new RunStarted(seed));
                for (int i = 0; i < 1200; i++) simulation.Tick(1f / 60f);
                return spawned.ToList();
            }

            var first = RunWithSeed(42);
            var again = RunWithSeed(42);
            var other = RunWithSeed(43);

            Assert.That(first, Has.Count.GreaterThan(10));
            Assert.That(again, Is.EqualTo(first));
            Assert.That(other, Is.Not.EqualTo(first));
        }

        [Test]
        public void OnceThePoolsAreWarm_ARunAllocatesNothing()
        {
            var block = FakeDefinition.Hazard("block");
            var shard = FakeDefinition.Pickup("shard", 10);
            var patterns = new ITrackPattern[]
            {
                new FakePattern("A", 0, 1f, new ITrackEntityDefinition[] { block, null, shard }, new ITrackEntityDefinition[] { null, block, null }),
                new FakePattern("B", 0, 1f, new ITrackEntityDefinition[] { shard, shard, shard }),
                new FakePattern("C", 0, 1f, new ITrackEntityDefinition[] { null, null, block }, new ITrackEntityDefinition[] { block, block, null }),
            };
            var random = new SeededRandom(7);
            var spawner = new TrackSpawner(_field, new WeightedPatternPicker(patterns, random), _lanes, _settings);
            // A bus of its own: the fixture's listeners build strings, which is not what is being measured.
            var bus = new MessageBus();
            int contacts = 0;
            bus.Subscribe<HazardHit>(_ => contacts++);
            bus.Subscribe<PickupCollected>(_ => contacts++);
            using var simulation = new TrackSimulation(_field, spawner, _progress, _body, bus, bus, _settings, random);
            _progress.Speed = 30f;
            const float step = 1f / 60f;

            // The runner stays in the centre lane and touches things, so contacts are part of what is measured.
            bus.Publish(new RunStarted(seed: 99));
            for (int i = 0; i < 3000; i++) simulation.Tick(step);

            Assert.That(() =>
            {
                for (int i = 0; i < 600; i++) simulation.Tick(step);
            }, Is.Not.AllocatingGCMemory());
            Assert.That(contacts, Is.GreaterThan(0), "The measured stretch should have included contacts.");
        }
    }
}
