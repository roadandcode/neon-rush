using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Effects.Logic;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Effects.Tests
{
    public sealed class EffectsDirectorTests
    {
        private sealed class FakeView : IEffectsView
        {
            public List<Vector3> Impacts { get; } = new List<Vector3>(64);

            public List<Vector3> Pickups { get; } = new List<Vector3>(64);

            public void PlayImpact(Vector3 position) => Impacts.Add(position);

            public void PlayPickup(Vector3 position) => Pickups.Add(position);
        }

        private sealed class FixedBody : IRunnerBody
        {
            public Bounds Bounds { get; set; }
        }

        private FakeView _view;
        private FixedBody _runner;
        private MessageBus _bus;
        private EffectsDirector _director;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeView();
            _runner = new FixedBody { Bounds = new Bounds(new Vector3(-2.5f, 0.9f, 0f), new Vector3(0.9f, 1.8f, 0.9f)) };
            _bus = new MessageBus();
            _director = new EffectsDirector(_view, _runner, _bus);
            _director.Start();
        }

        [Test]
        public void AHit_BurstsWhereTheRunnerIs()
        {
            _bus.Publish(new HazardHit());

            Assert.That(_view.Impacts, Is.EqualTo(new[] { new Vector3(-2.5f, 0.9f, 0f) }));
            Assert.That(_view.Pickups, Is.Empty);
        }

        [Test]
        public void APickup_SparklesWhereTheRunnerIs_EvenMidJump()
        {
            _runner.Bounds = new Bounds(new Vector3(2.5f, 2.2f, 0f), new Vector3(0.9f, 1.8f, 0.9f));

            _bus.Publish(new PickupCollected(10));

            Assert.That(_view.Pickups, Is.EqualTo(new[] { new Vector3(2.5f, 2.2f, 0f) }));
            Assert.That(_view.Impacts, Is.Empty);
        }

        [Test]
        public void BeforeStart_AndAfterDispose_NothingPlays()
        {
            var view = new FakeView();
            var director = new EffectsDirector(view, _runner, _bus);
            _bus.Publish(new PickupCollected(10));
            Assert.That(view.Pickups, Is.Empty);

            director.Start();
            director.Dispose();
            _bus.Publish(new PickupCollected(10));
            Assert.That(view.Pickups, Is.Empty);
        }

        [Test]
        public void AStreakOfPickups_AllocatesNothing()
        {
            _bus.Publish(new PickupCollected(10));

            Assert.That(() =>
            {
                for (int i = 0; i < 40; i++) _bus.Publish(new PickupCollected(10));
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
