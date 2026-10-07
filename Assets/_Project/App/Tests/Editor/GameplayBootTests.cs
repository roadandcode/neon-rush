using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.App.Composition;
using RoadAndCode.NeonRush.App.Flow;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;

namespace RoadAndCode.NeonRush.App.Tests
{
    public sealed class GameplayBootTests
    {
        /// <summary>A start-up task the test finishes by hand.</summary>
        private sealed class ManualTask : IStartupTask
        {
            private readonly AwaitableCompletionSource _completion = new AwaitableCompletionSource();
            private readonly List<string> _log;
            private readonly string _name;

            public ManualTask(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public Awaitable RunAsync(CancellationToken cancellation)
            {
                _log.Add("started " + _name);
                return _completion.Awaitable;
            }

            public void Finish()
            {
                _log.Add("finished " + _name);
                _completion.SetResult();
            }
        }

        private sealed class OneSeed : IRunSeedSource
        {
            public int NextSeed() => 1;
        }

        private GameFlow _flow;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _flow = new GameFlow(new MessageBus(), new OneSeed(), new FlowSettings(introSeconds: 0f));
            _flow.Start();
            _log = new List<string>();
        }

        [Test]
        public void WithNothingToWaitFor_TheMenuOpensAtOnce()
        {
            var boot = new GameplayBoot(_flow, new IStartupTask[0]);

            _ = boot.StartAsync(CancellationToken.None);

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Menu));
        }

        [Test]
        public void TheMenuStaysShut_UntilEveryTaskHasFinished()
        {
            var content = new ManualTask("content", _log);
            var settings = new ManualTask("settings", _log);
            var boot = new GameplayBoot(_flow, new IStartupTask[] { content, settings });

            _ = boot.StartAsync(CancellationToken.None);
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Boot));

            content.Finish();
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Boot), "One task is still running.");

            settings.Finish();
            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Menu));
        }

        [Test]
        public void TasksRunOneAfterAnother_InTheOrderTheyWereRegistered()
        {
            var first = new ManualTask("first", _log);
            var second = new ManualTask("second", _log);
            var boot = new GameplayBoot(_flow, new IStartupTask[] { first, second });

            _ = boot.StartAsync(CancellationToken.None);
            first.Finish();
            second.Finish();

            Assert.That(_log, Is.EqualTo(new[] { "started first", "finished first", "started second", "finished second" }));
        }

        [Test]
        public void ACancelledBoot_NeverOpensTheMenu()
        {
            using var cancellation = new CancellationTokenSource();
            var content = new ManualTask("content", _log);
            var boot = new GameplayBoot(_flow, new IStartupTask[] { content });

            _ = boot.StartAsync(cancellation.Token);
            cancellation.Cancel();
            content.Finish();

            Assert.That(_flow.Phase, Is.EqualTo(GamePhase.Boot));
        }
    }
}
