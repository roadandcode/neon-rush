using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Simulation;

namespace RoadAndCode.Core.Tests
{
    public sealed class SimulationLoopTests
    {
        private sealed class Probe : ISimulationSystem
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Probe(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public float LastDeltaTime { get; private set; }

            public void Tick(float deltaTime)
            {
                LastDeltaTime = deltaTime;
                _log.Add(_name);
            }
        }

        [Test]
        public void Tick_RunsSystemsInTheOrderGiven()
        {
            var log = new List<string>();
            var loop = new SimulationLoop(new[] { new Probe("input", log), new Probe("move", log), new Probe("score", log) })
            {
                IsRunning = true,
            };

            loop.Tick(0.02f);

            Assert.That(log, Is.EqualTo(new[] { "input", "move", "score" }));
        }

        [Test]
        public void Tick_PassesDeltaTimeThrough()
        {
            var probe = new Probe("only", new List<string>());
            var loop = new SimulationLoop(new[] { probe }) { IsRunning = true };

            loop.Tick(0.25f);

            Assert.That(probe.LastDeltaTime, Is.EqualTo(0.25f));
        }

        [Test]
        public void Tick_WhileNotRunning_TouchesNothing()
        {
            var log = new List<string>();
            var loop = new SimulationLoop(new[] { new Probe("move", log) });

            loop.Tick(0.02f);

            Assert.That(loop.IsRunning, Is.False);
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void LaterChangesToTheSourceCollection_DoNotAffectTheLoop()
        {
            var log = new List<string>();
            var systems = new List<ISimulationSystem> { new Probe("first", log) };
            var loop = new SimulationLoop(systems) { IsRunning = true };

            systems.Add(new Probe("added-later", log));
            loop.Tick(0.02f);

            Assert.That(loop.SystemCount, Is.EqualTo(1));
            Assert.That(log, Is.EqualTo(new[] { "first" }));
        }
    }
}
