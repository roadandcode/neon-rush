using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Pace.Logic;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.Pace.Tests
{
    public sealed class RunProgressTests
    {
        /// <summary>Speed rises by one metre per second every second; a new tier every ten seconds.</summary>
        private sealed class LinearCurve : IDifficultyCurve
        {
            public const float StartSpeed = 10f;
            public const float TierLength = 10f;

            public float SpeedAt(float elapsed) => StartSpeed + elapsed;

            public int TierAt(float elapsed) => (int)(elapsed / TierLength);
        }

        private MessageBus _bus;
        private RunProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _bus = new MessageBus();
            _progress = new RunProgress(new LinearCurve(), _bus);
        }

        [Test]
        public void BeforeTheFirstTick_ReportsTheStartingSpeed()
        {
            Assert.That(_progress.Speed, Is.EqualTo(LinearCurve.StartSpeed));
            Assert.That(_progress.Distance, Is.Zero);
            Assert.That(_progress.Tier, Is.Zero);
        }

        [Test]
        public void Tick_AdvancesTimeAndCoversDistanceAtTheCurrentSpeed()
        {
            _progress.Tick(0.5f);

            Assert.That(_progress.Elapsed, Is.EqualTo(0.5f));
            Assert.That(_progress.Speed, Is.EqualTo(LinearCurve.StartSpeed + 0.5f));
            Assert.That(_progress.Distance, Is.EqualTo(_progress.Speed * 0.5f).Within(1e-4f));
        }

        [Test]
        public void Tier_FollowsTheCurve()
        {
            for (int i = 0; i < 25; i++) _progress.Tick(1f);

            Assert.That(_progress.Tier, Is.EqualTo(2));
        }

        [Test]
        public void RunStarted_ResetsEverything()
        {
            for (int i = 0; i < 25; i++) _progress.Tick(1f);

            _bus.Publish(new RunStarted(seed: 1));

            Assert.That(_progress.Elapsed, Is.Zero);
            Assert.That(_progress.Distance, Is.Zero);
            Assert.That(_progress.Speed, Is.EqualTo(LinearCurve.StartSpeed));
            Assert.That(_progress.Tier, Is.Zero);
        }

        [Test]
        public void AfterDispose_RunStartedNoLongerResets()
        {
            _progress.Tick(1f);
            _progress.Dispose();

            _bus.Publish(new RunStarted(seed: 1));

            Assert.That(_progress.Elapsed, Is.EqualTo(1f));
        }
    }
}
