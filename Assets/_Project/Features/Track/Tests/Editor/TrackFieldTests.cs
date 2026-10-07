using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.NeonRush.Track.Logic;

namespace RoadAndCode.NeonRush.Track.Tests
{
    public sealed class TrackFieldTests
    {
        private TrackField _field;
        private List<string> _events;
        private FakeDefinition _block;

        [SetUp]
        public void SetUp()
        {
            _field = new TrackField();
            _events = new List<string>();
            _block = FakeDefinition.Hazard("block");
            _field.Added += entity => _events.Add("added " + entity.Id);
            _field.Removed += entity => _events.Add("removed " + entity.Id);
        }

        [Test]
        public void Spawn_PlacesTheEntity_AndAnnouncesIt()
        {
            var entity = _field.Spawn(_block, x: 2.5f, z: 40f);

            Assert.That(_field.Entities, Has.Count.EqualTo(1));
            Assert.That(entity.Definition, Is.SameAs(_block));
            Assert.That(entity.X, Is.EqualTo(2.5f));
            Assert.That(entity.Z, Is.EqualTo(40f));
            Assert.That(entity.Touched, Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "added 0" }));
        }

        [Test]
        public void RemovedEntities_AreReused_WithTheirStateCleared()
        {
            var first = _field.Spawn(_block, 0f, 10f);
            first.MarkTouched();
            _field.RemoveAt(0);

            var second = _field.Spawn(_block, 2.5f, 60f);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Touched, Is.False);
            Assert.That(second.Z, Is.EqualTo(60f));
            Assert.That(_field.Capacity, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAt_KeepsEveryOtherEntity()
        {
            var a = _field.Spawn(_block, 0f, 10f);
            var b = _field.Spawn(_block, 0f, 20f);
            var c = _field.Spawn(_block, 0f, 30f);

            _field.RemoveAt(0);

            Assert.That(_field.Entities, Is.EquivalentTo(new[] { b, c }));
            Assert.That(_events, Does.Contain("removed " + a.Id));
        }

        [Test]
        public void Clear_RemovesAndAnnouncesEverything()
        {
            _field.Spawn(_block, 0f, 10f);
            _field.Spawn(_block, 0f, 20f);
            _events.Clear();

            _field.Clear();

            Assert.That(_field.Entities, Is.Empty);
            Assert.That(_events, Is.EquivalentTo(new[] { "removed 0", "removed 1" }));
        }
    }
}
