using System;
using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.Core.Tests
{
    public sealed class MessageBusTests
    {
        private readonly struct Ping
        {
            public readonly int Value;
            public Ping(int value) => Value = value;
        }

        private readonly struct Pong
        {
        }

        [Test]
        public void Publish_DeliversToEverySubscriberInSubscriptionOrder()
        {
            var bus = new MessageBus();
            var received = new List<string>();
            bus.Subscribe<Ping>(ping => received.Add("first:" + ping.Value));
            bus.Subscribe<Ping>(ping => received.Add("second:" + ping.Value));

            bus.Publish(new Ping(7));

            Assert.That(received, Is.EqualTo(new[] { "first:7", "second:7" }));
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNothing()
        {
            var bus = new MessageBus();

            Assert.DoesNotThrow(() => bus.Publish(new Ping(1)));
        }

        [Test]
        public void Publish_OnlyReachesSubscribersOfThatMessageType()
        {
            var bus = new MessageBus();
            int pings = 0, pongs = 0;
            bus.Subscribe<Ping>(_ => pings++);
            bus.Subscribe<Pong>(_ => pongs++);

            bus.Publish(new Ping(1));

            Assert.That(pings, Is.EqualTo(1));
            Assert.That(pongs, Is.Zero);
        }

        [Test]
        public void DisposingSubscription_StopsDelivery_AndIsSafeToRepeat()
        {
            var bus = new MessageBus();
            int count = 0;
            IDisposable subscription = bus.Subscribe<Ping>(_ => count++);

            subscription.Dispose();
            subscription.Dispose();
            bus.Publish(new Ping(1));

            Assert.That(count, Is.Zero);
        }

        [Test]
        public void HandlerCanUnsubscribeItself_WithoutSkippingOthers()
        {
            var bus = new MessageBus();
            int first = 0, second = 0;
            IDisposable subscription = null;
            subscription = bus.Subscribe<Ping>(_ =>
            {
                first++;
                subscription.Dispose();
            });
            bus.Subscribe<Ping>(_ => second++);

            bus.Publish(new Ping(1));
            bus.Publish(new Ping(2));

            Assert.That(first, Is.EqualTo(1));
            Assert.That(second, Is.EqualTo(2));
        }

        [Test]
        public void HandlerSubscribedDuringPublish_StartsWithTheNextMessage()
        {
            var bus = new MessageBus();
            int late = 0;
            bool added = false;
            bus.Subscribe<Ping>(_ =>
            {
                if (added) return;
                added = true;
                bus.Subscribe<Ping>(__ => late++);
            });

            bus.Publish(new Ping(1));
            Assert.That(late, Is.Zero);

            bus.Publish(new Ping(2));
            Assert.That(late, Is.EqualTo(1));
        }

        [Test]
        public void Subscribe_NullHandler_Throws()
        {
            var bus = new MessageBus();

            Assert.Throws<ArgumentNullException>(() => bus.Subscribe<Ping>(null));
        }

        [Test]
        public void Publish_AllocatesNothing()
        {
            var bus = new MessageBus();
            int total = 0;
            bus.Subscribe<Ping>(ping => total += ping.Value);
            bus.Publish(new Ping(1));

            Assert.That(() =>
            {
                for (int i = 0; i < 100; i++) bus.Publish(new Ping(i));
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
