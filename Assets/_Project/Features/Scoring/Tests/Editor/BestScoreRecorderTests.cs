using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.NeonRush.Scoring.Data;
using RoadAndCode.NeonRush.Scoring.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Scoring;

namespace RoadAndCode.NeonRush.Scoring.Tests
{
    public sealed class BestScoreRecorderTests
    {
        private FixedProgress _progress;
        private MessageBus _bus;
        private InMemorySaveStore _store;
        private ScoreKeeper _keeper;
        private BestScoreRecorder _recorder;
        private List<RunScored> _results;

        [SetUp]
        public void SetUp()
        {
            _progress = new FixedProgress();
            _bus = new MessageBus();
            _store = new InMemorySaveStore();
            _keeper = new ScoreKeeper(_progress, new ScoringRules(), _bus, _bus);
            _recorder = new BestScoreRecorder(_keeper, _store, _bus, _bus);
            _results = new List<RunScored>();
            _bus.Subscribe<RunScored>(_results.Add);
        }

        private void FinishRunWithDistance(float metres, RunEndReason reason = RunEndReason.Crashed)
        {
            _bus.Publish(new RunStarted(seed: 1));
            _progress.Distance = metres;
            _keeper.Tick(0.1f);
            _bus.Publish(new RunEnded(reason));
        }

        [Test]
        public void WithNothingSaved_TheBestIsZero()
        {
            Assert.That(_recorder.Best, Is.Zero);
        }

        [Test]
        public void TheFirstRun_SetsTheBest()
        {
            FinishRunWithDistance(120f);

            Assert.That(_recorder.Best, Is.EqualTo(120));
            Assert.That(_results, Has.Count.EqualTo(1));
            Assert.That(_results[0].Score, Is.EqualTo(120));
            Assert.That(_results[0].Best, Is.EqualTo(120));
            Assert.That(_results[0].IsNewBest, Is.True);
        }

        [Test]
        public void AWorseRun_LeavesTheBestAlone()
        {
            FinishRunWithDistance(120f);

            FinishRunWithDistance(80f);

            Assert.That(_recorder.Best, Is.EqualTo(120));
            Assert.That(_results[1].Score, Is.EqualTo(80));
            Assert.That(_results[1].Best, Is.EqualTo(120));
            Assert.That(_results[1].IsNewBest, Is.False);
        }

        [Test]
        public void EqualingTheBest_IsNotANewBest()
        {
            FinishRunWithDistance(120f);

            FinishRunWithDistance(120f);

            Assert.That(_results[1].IsNewBest, Is.False);
        }

        [Test]
        public void AnAbandonedRun_StillCounts()
        {
            FinishRunWithDistance(200f, RunEndReason.Abandoned);

            Assert.That(_recorder.Best, Is.EqualTo(200));
        }

        [Test]
        public void TheBest_SurvivesANewRecorderOnTheSameStore()
        {
            FinishRunWithDistance(150f);
            _recorder.Dispose();

            var later = new BestScoreRecorder(_keeper, _store, _bus, _bus);

            Assert.That(later.Best, Is.EqualTo(150));
        }
    }
}
