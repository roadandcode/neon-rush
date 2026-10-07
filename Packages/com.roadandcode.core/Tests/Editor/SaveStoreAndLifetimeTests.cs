using System;
using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Persistence;

namespace RoadAndCode.Core.Tests
{
    public sealed class InMemorySaveStoreTests
    {
        [Serializable]
        private sealed class Profile
        {
            public int BestScore;
        }

        [Test]
        public void SaveThenLoad_ReturnsTheRecord()
        {
            var store = new InMemorySaveStore();
            store.Save("profile", new Profile { BestScore = 320 });

            bool found = store.TryLoad("profile", out Profile loaded);

            Assert.That(found, Is.True);
            Assert.That(loaded.BestScore, Is.EqualTo(320));
        }

        [Test]
        public void Load_MissingKey_ReportsNotFound()
        {
            var store = new InMemorySaveStore();

            bool found = store.TryLoad("nope", out Profile loaded);

            Assert.That(found, Is.False);
            Assert.That(loaded, Is.Null);
        }

        [Test]
        public void Load_RecordOfAnotherType_ReportsNotFound()
        {
            var store = new InMemorySaveStore();
            store.Save("profile", "not a profile");

            Assert.That(store.TryLoad("profile", out Profile _), Is.False);
        }

        [Test]
        public void Delete_RemovesTheRecord()
        {
            var store = new InMemorySaveStore();
            store.Save("profile", new Profile());

            store.Delete("profile");

            Assert.That(store.TryLoad("profile", out Profile _), Is.False);
        }
    }

    public sealed class CompositeDisposableTests
    {
        private sealed class Flag : IDisposable
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Flag(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Dispose() => _log.Add(_name);
        }

        [Test]
        public void Dispose_ReleasesEverythingOnce_NewestFirst()
        {
            var log = new List<string>();
            var owner = new CompositeDisposable();
            new Flag("a", log).AddTo(owner);
            new Flag("b", log).AddTo(owner);

            owner.Dispose();
            owner.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void Add_AfterDispose_ReleasesTheItemImmediately()
        {
            var log = new List<string>();
            var owner = new CompositeDisposable();
            owner.Dispose();

            owner.Add(new Flag("late", log));

            Assert.That(log, Is.EqualTo(new[] { "late" }));
        }
    }
}
