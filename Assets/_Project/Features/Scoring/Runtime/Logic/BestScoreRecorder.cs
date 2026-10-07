using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Scoring;
using UnityEngine;

namespace RoadAndCode.NeonRush.Scoring.Logic
{
    /// <summary>
    /// When a run ends, compares its score with the stored best, saves a new best, and
    /// announces the result. Abandoned runs count too: the points were earned.
    /// </summary>
    internal sealed class BestScoreRecorder : IBestScore, IDisposable
    {
        private const string SaveKey = "best-score";

        private readonly ScoreKeeper _keeper;
        private readonly ISaveStore _store;
        private readonly IPublisher _publisher;
        private readonly IDisposable _subscription;

        public BestScoreRecorder(ScoreKeeper keeper, ISaveStore store, IPublisher publisher, ISubscriber subscriber)
        {
            _keeper = Guard.NotNull(keeper, nameof(keeper));
            _store = Guard.NotNull(store, nameof(store));
            _publisher = Guard.NotNull(publisher, nameof(publisher));
            _subscription = Guard.NotNull(subscriber, nameof(subscriber)).Subscribe<RunEnded>(OnRunEnded);
        }

        public int Best => _store.TryLoad(SaveKey, out BestScoreRecord record) ? record.Best : 0;

        public void Dispose() => _subscription.Dispose();

        private void OnRunEnded(RunEnded message)
        {
            int score = _keeper.Score;
            int previousBest = Best;
            bool isNewBest = score > previousBest;

            if (isNewBest) _store.Save(SaveKey, new BestScoreRecord(score));
            _publisher.Publish(new RunScored(score, Mathf.Max(score, previousBest), isNewBest));
        }

        [Serializable]
        private sealed class BestScoreRecord
        {
            [SerializeField] private int _best;

            public BestScoreRecord(int best)
            {
                _best = best;
            }

            public int Best => _best;
        }
    }
}
