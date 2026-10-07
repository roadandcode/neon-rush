using System;
using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Randomness;
using RoadAndCode.NeonRush.Track.Logic;

namespace RoadAndCode.NeonRush.Track.Tests
{
    public sealed class WeightedPatternPickerTests
    {
        private static FakePattern Pattern(string name, int minTier = 0, float weight = 1f)
        {
            return new FakePattern(name, minTier, weight, new ITrackEntityDefinition[] { null, null, null });
        }

        [Test]
        public void OnlyPatternsUnlockedAtTheTier_ArePicked()
        {
            var easy = Pattern("easy");
            var hard = Pattern("hard", minTier: 2);
            var picker = new WeightedPatternPicker(new[] { easy, hard }, new SeededRandom(5));

            for (int i = 0; i < 50; i++)
            {
                Assert.That(picker.Next(tier: 1), Is.SameAs(easy));
            }
        }

        [Test]
        public void TheSamePattern_IsNotPickedTwiceInARow_WhenThereIsAChoice()
        {
            var patterns = new[] { Pattern("a"), Pattern("b"), Pattern("c") };
            var picker = new WeightedPatternPicker(patterns, new SeededRandom(11));

            ITrackPattern previous = null;
            for (int i = 0; i < 200; i++)
            {
                var next = picker.Next(tier: 0);
                Assert.That(next, Is.Not.SameAs(previous));
                previous = next;
            }
        }

        [Test]
        public void TheOnlyEligiblePattern_Repeats()
        {
            var only = Pattern("only");
            var picker = new WeightedPatternPicker(new[] { only, Pattern("later", minTier: 3) }, new SeededRandom(1));

            Assert.That(picker.Next(0), Is.SameAs(only));
            Assert.That(picker.Next(0), Is.SameAs(only));
        }

        [Test]
        public void HeavierPatterns_ArePickedMoreOften()
        {
            var common = Pattern("common", weight: 3f);
            var filler = Pattern("filler", weight: 1f);
            var rare = Pattern("rare", weight: 1f);
            var picker = new WeightedPatternPicker(new[] { common, filler, rare }, new SeededRandom(2024));
            var counts = new Dictionary<ITrackPattern, int> { { common, 0 }, { filler, 0 }, { rare, 0 } };

            for (int i = 0; i < 3000; i++) counts[picker.Next(0)]++;

            Assert.That(counts[common], Is.GreaterThan(counts[filler]));
            Assert.That(counts[common], Is.GreaterThan(counts[rare]));
        }

        [Test]
        public void TheRoll_SelectsByCumulativeWeight()
        {
            var a = Pattern("a", weight: 1f);
            var b = Pattern("b", weight: 3f);
            var low = new WeightedPatternPicker(new[] { a, b }, new ScriptedRandom(0.1f));
            var high = new WeightedPatternPicker(new[] { a, b }, new ScriptedRandom(0.9f));

            Assert.That(low.Next(0), Is.SameAs(a));
            Assert.That(high.Next(0), Is.SameAs(b));
        }

        [Test]
        public void Reset_AllowsThePreviousPatternAgain()
        {
            var a = Pattern("a");
            var b = Pattern("b");
            var picker = new WeightedPatternPicker(new[] { a, b }, new ScriptedRandom(0f));
            Assert.That(picker.Next(0), Is.SameAs(a));

            picker.Reset();

            Assert.That(picker.Next(0), Is.SameAs(a));
        }

        [Test]
        public void ALibraryWithNothingAtTierZero_IsRejected()
        {
            Assert.Throws<InvalidOperationException>(
                () => new WeightedPatternPicker(new[] { Pattern("late", minTier: 1) }, new SeededRandom(1)));
        }

        [Test]
        public void AnEmptyPattern_IsRejected()
        {
            var empty = new FakePattern("empty", 0, 1f);

            Assert.Throws<InvalidOperationException>(
                () => new WeightedPatternPicker(new ITrackPattern[] { empty }, new SeededRandom(1)));
        }
    }
}
