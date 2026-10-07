using System.Linq;
using NUnit.Framework;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Data;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Track.Tests
{
    public sealed class TrackSpawnerTests
    {
        private const float Speed = 20f;
        private const float Tolerance = 1e-3f;

        private TrackSettings _settings;
        private LaneGrid _lanes;
        private TrackField _field;
        private FakeDefinition _block;
        private FakeDefinition _shard;
        private SinglePatternPicker _picker;
        private TrackSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _settings = new TrackSettings();
            _lanes = new LaneGrid(3, 2.5f);
            _field = new TrackField();
            _block = FakeDefinition.Hazard("block");
            _shard = FakeDefinition.Pickup("shard", 10);

            // Two rows: a block on the left, then shards on the centre and right.
            var pattern = new FakePattern("pair", 0, 1f,
                new ITrackEntityDefinition[] { _block, null, null },
                new ITrackEntityDefinition[] { null, _shard, _shard });

            _picker = new SinglePatternPicker(pattern);
            _spawner = new TrackSpawner(_field, _picker, _lanes, _settings);
        }

        private float RowGap => Mathf.Max(_settings.MinRowGap, Speed * _settings.RowGapSeconds);

        private float PatternGap => Mathf.Max(_settings.MinRowGap, Speed * _settings.PatternGapSeconds);

        [Test]
        public void NothingSpawns_UntilTheStartDelayHasBeenCovered()
        {
            _spawner.Advance(_settings.StartDelayDistance - 0.5f, Speed, tier: 0);

            Assert.That(_field.Entities, Is.Empty);
        }

        [Test]
        public void TheFirstRow_AppearsAtTheSpawnLine_InTheRightLanes()
        {
            _spawner.Advance(_settings.StartDelayDistance, Speed, tier: 0);

            Assert.That(_field.Entities, Has.Count.EqualTo(1));
            var entity = _field.Entities[0];
            Assert.That(entity.Definition, Is.SameAs(_block));
            Assert.That(entity.X, Is.EqualTo(_lanes.CenterOf(0)));
            Assert.That(entity.Z, Is.EqualTo(_settings.SpawnDistance).Within(Tolerance));
        }

        [Test]
        public void RowsWithinAPattern_AreSpacedByTravelTime()
        {
            _spawner.Advance(_settings.StartDelayDistance, Speed, tier: 0);

            _spawner.Advance(RowGap - 0.5f, Speed, tier: 0);
            Assert.That(_field.Entities, Has.Count.EqualTo(1), "The second row is not due yet.");

            _spawner.Advance(0.5f, Speed, tier: 0);
            Assert.That(_field.Entities, Has.Count.EqualTo(3));
            Assert.That(_field.Entities.Skip(1).Select(e => e.X), Is.EquivalentTo(new[] { _lanes.CenterOf(1), _lanes.CenterOf(2) }));
        }

        [Test]
        public void ARowThatWasDueMidTick_IsPlacedCloserByTheOvershoot()
        {
            const float overshoot = 1.5f;

            _spawner.Advance(_settings.StartDelayDistance + overshoot, Speed, tier: 0);

            Assert.That(_field.Entities[0].Z, Is.EqualTo(_settings.SpawnDistance - overshoot).Within(Tolerance));
        }

        [Test]
        public void TheGapAfterAPattern_IsThePatternGap()
        {
            _spawner.Advance(_settings.StartDelayDistance, Speed, tier: 0);
            _spawner.Advance(RowGap, Speed, tier: 0);
            int afterFirstPattern = _field.Entities.Count;

            _spawner.Advance(PatternGap - 0.5f, Speed, tier: 0);
            Assert.That(_field.Entities, Has.Count.EqualTo(afterFirstPattern), "Still inside the gap between patterns.");

            _spawner.Advance(0.5f, Speed, tier: 0);
            Assert.That(_field.Entities, Has.Count.EqualTo(afterFirstPattern + 1));
        }

        [Test]
        public void SlowRuns_NeverPackRowsCloserThanTheMinimumGap()
        {
            const float crawl = 1f;
            _spawner.Advance(_settings.StartDelayDistance, crawl, tier: 0);

            _spawner.Advance(_settings.MinRowGap - 0.1f, crawl, tier: 0);

            Assert.That(_field.Entities, Has.Count.EqualTo(1));
        }

        [Test]
        public void OneLongTick_SpawnsEveryRowThatBecameDue()
        {
            _spawner.Advance(_settings.StartDelayDistance + RowGap + PatternGap, Speed, tier: 0);

            // Row one (1 entity), row two (2 entities), then row one of the next pattern (1 entity).
            Assert.That(_field.Entities, Has.Count.EqualTo(4));
        }

        [Test]
        public void Reset_RestoresTheStartDelay_AndResetsThePicker()
        {
            _spawner.Advance(_settings.StartDelayDistance, Speed, tier: 0);
            _field.Clear();
            int resetsBefore = _picker.Resets;

            _spawner.Reset();
            _spawner.Advance(_settings.StartDelayDistance - 0.5f, Speed, tier: 0);

            Assert.That(_field.Entities, Is.Empty);
            Assert.That(_picker.Resets, Is.EqualTo(resetsBefore + 1));
        }
    }
}
