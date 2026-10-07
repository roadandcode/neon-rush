using System;
using NUnit.Framework;
using RoadAndCode.Core.Persistence;
using UnityEngine;
using UnityEngine.TestTools;

namespace RoadAndCode.Core.Tests
{
    public sealed class PlayerPrefsSaveStoreTests
    {
        // A prefix of its own, so the tests can never touch a real save.
        private const string Prefix = "roadandcode.core.tests.";
        private const string Key = "profile";

        /// <summary>Shaped like the records games actually store: private state, no default constructor.</summary>
        [Serializable]
        private sealed class Profile
        {
            [SerializeField] private int _best;
            [SerializeField] private string _name;

            public Profile(int best, string name)
            {
                _best = best;
                _name = name;
            }

            public int Best => _best;

            public string Name => _name;
        }

        private PlayerPrefsSaveStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new PlayerPrefsSaveStore(Prefix);
            _store.Delete(Key);
        }

        [TearDown]
        public void TearDown()
        {
            _store.Delete(Key);
        }

        [Test]
        public void SaveThenLoad_RoundTripsARecordWithPrivateFields()
        {
            _store.Save(Key, new Profile(4200, "Ravi"));

            bool found = _store.TryLoad(Key, out Profile loaded);

            Assert.That(found, Is.True);
            Assert.That(loaded.Best, Is.EqualTo(4200));
            Assert.That(loaded.Name, Is.EqualTo("Ravi"));
        }

        [Test]
        public void Load_WithNothingSaved_ReportsNotFound()
        {
            Assert.That(_store.TryLoad(Key, out Profile _), Is.False);
        }

        [Test]
        public void Delete_RemovesTheRecord()
        {
            _store.Save(Key, new Profile(1, "x"));

            _store.Delete(Key);

            Assert.That(_store.TryLoad(Key, out Profile _), Is.False);
        }

        [Test]
        public void Load_UnreadableData_IsReportedAsMissing_WithAWarning()
        {
            PlayerPrefs.SetString(Prefix + Key, "{ this is not json");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("could not be read"));

            Assert.That(_store.TryLoad(Key, out Profile _), Is.False);
        }

        [Test]
        public void StoresWithDifferentPrefixes_DoNotSeeEachOther()
        {
            var other = new PlayerPrefsSaveStore(Prefix + "other.");
            _store.Save(Key, new Profile(7, "mine"));

            Assert.That(other.TryLoad(Key, out Profile _), Is.False);
        }
    }
}
