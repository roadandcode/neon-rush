using System;
using NUnit.Framework;
using RoadAndCode.Core.Randomness;

namespace RoadAndCode.Core.Tests
{
    public sealed class SeededRandomTests
    {
        [Test]
        public void SameSeed_GivesTheSameSequence()
        {
            var a = new SeededRandom(1234);
            var b = new SeededRandom(1234);

            for (int i = 0; i < 100; i++)
            {
                Assert.That(a.NextInt(0, 1000), Is.EqualTo(b.NextInt(0, 1000)));
            }
        }

        [Test]
        public void DifferentSeeds_Diverge()
        {
            var a = new SeededRandom(1);
            var b = new SeededRandom(2);
            int same = 0;

            for (int i = 0; i < 100; i++)
            {
                if (a.NextInt(0, 1000) == b.NextInt(0, 1000)) same++;
            }

            Assert.That(same, Is.LessThan(10));
        }

        [Test]
        public void Reseed_RestartsTheSequence()
        {
            var random = new SeededRandom(99);
            int first = random.NextInt(0, int.MaxValue);
            random.NextInt(0, int.MaxValue);

            random.Reseed(99);

            Assert.That(random.NextInt(0, int.MaxValue), Is.EqualTo(first));
        }

        [Test]
        public void ZeroSeed_StillProducesVariedValues()
        {
            var random = new SeededRandom(0);

            Assert.That(random.NextInt(0, int.MaxValue), Is.Not.EqualTo(random.NextInt(0, int.MaxValue)));
        }

        [Test]
        public void NextFloat_StaysInZeroInclusiveToOneExclusive()
        {
            var random = new SeededRandom(42);

            for (int i = 0; i < 10000; i++)
            {
                float value = random.NextFloat();
                Assert.That(value, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            }
        }

        [Test]
        public void NextInt_StaysInRange_AndReachesBothEnds()
        {
            var random = new SeededRandom(42);
            bool sawMin = false, sawMax = false;

            for (int i = 0; i < 2000; i++)
            {
                int value = random.NextInt(-2, 3);
                Assert.That(value, Is.InRange(-2, 2));
                sawMin |= value == -2;
                sawMax |= value == 2;
            }

            Assert.That(sawMin && sawMax, Is.True);
        }

        [Test]
        public void NextInt_EmptyRange_Throws()
        {
            var random = new SeededRandom(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(5, 5));
        }
    }
}
