using System;
using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.StateMachines;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.Core.Tests
{
    public sealed class StateMachineTests
    {
        private enum Light
        {
            Red,
            Green,
            Amber,
            Off,
        }

        private sealed class Recorder : State
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Recorder(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public Action OnEnter { get; set; }

            public override void Enter()
            {
                _log.Add("enter " + _name);
                OnEnter?.Invoke();
            }

            public override void Tick(float deltaTime) => _log.Add("tick " + _name);

            public override void Exit() => _log.Add("exit " + _name);
        }

        private static StateMachine<Light> TrafficLight(List<string> log = null, Recorder green = null)
        {
            log = log ?? new List<string>();
            return new StateMachine<Light>()
                .AddState(Light.Red, new Recorder("red", log))
                .AddState(Light.Green, green ?? new Recorder("green", log))
                .AddState(Light.Amber, new Recorder("amber", log))
                .AddState(Light.Off)
                .Allow(Light.Red, Light.Green)
                .Allow(Light.Green, Light.Amber)
                .Allow(Light.Amber, Light.Red)
                .AllowFromAny(Light.Off);
        }

        [Test]
        public void Start_EntersInitialState()
        {
            var log = new List<string>();
            var machine = TrafficLight(log);

            machine.Start(Light.Red);

            Assert.That(machine.Current, Is.EqualTo(Light.Red));
            Assert.That(machine.IsIn(Light.Red), Is.True);
            Assert.That(log, Is.EqualTo(new[] { "enter red" }));
        }

        [Test]
        public void IsIn_BeforeStart_IsFalseEvenForTheDefaultKey()
        {
            var machine = TrafficLight();

            Assert.That(machine.IsIn(Light.Red), Is.False);
        }

        [Test]
        public void TryGo_DeclaredTransition_ExitsThenEnters_AndRaisesChanged()
        {
            var log = new List<string>();
            var machine = TrafficLight(log);
            var changes = new List<(Light, Light)>();
            machine.Changed += (from, to) => changes.Add((from, to));
            machine.Start(Light.Red);
            log.Clear();

            bool moved = machine.TryGo(Light.Green);

            Assert.That(moved, Is.True);
            Assert.That(log, Is.EqualTo(new[] { "exit red", "enter green" }));
            Assert.That(changes, Is.EqualTo(new[] { (Light.Red, Light.Green) }));
        }

        [Test]
        public void TryGo_UndeclaredTransition_IsRejectedAndStateIsUnchanged()
        {
            var machine = TrafficLight();
            machine.Start(Light.Red);

            bool moved = machine.TryGo(Light.Amber);

            Assert.That(moved, Is.False);
            Assert.That(machine.Current, Is.EqualTo(Light.Red));
        }

        [Test]
        public void TryGo_ToTheCurrentState_IsRejected()
        {
            var machine = TrafficLight();
            machine.Start(Light.Red);

            Assert.That(machine.TryGo(Light.Red), Is.False);
        }

        [Test]
        public void Go_UndeclaredTransition_Throws()
        {
            var machine = TrafficLight();
            machine.Start(Light.Red);

            Assert.Throws<InvalidOperationException>(() => machine.Go(Light.Amber));
        }

        [Test]
        public void AllowFromAny_IsReachableFromEveryState()
        {
            var machine = TrafficLight();
            machine.Start(Light.Red);
            machine.Go(Light.Green);

            Assert.That(machine.TryGo(Light.Off), Is.True);
            Assert.That(machine.Current, Is.EqualTo(Light.Off));
        }

        [Test]
        public void ChangeRequestedInsideEnter_RunsAfterTheCurrentChangeCompletes()
        {
            var log = new List<string>();
            var green = new Recorder("green", log);
            var machine = TrafficLight(log, green);
            green.OnEnter = () => machine.TryGo(Light.Amber);
            machine.Start(Light.Red);
            log.Clear();

            machine.Go(Light.Green);

            Assert.That(machine.Current, Is.EqualTo(Light.Amber));
            Assert.That(log, Is.EqualTo(new[] { "exit red", "enter green", "exit green", "enter amber" }));
        }

        [Test]
        public void Tick_ReachesOnlyTheCurrentState()
        {
            var log = new List<string>();
            var machine = TrafficLight(log);
            machine.Start(Light.Red);
            machine.Go(Light.Green);
            log.Clear();

            machine.Tick(0.016f);

            Assert.That(log, Is.EqualTo(new[] { "tick green" }));
        }

        [Test]
        public void StateWithoutBehaviour_CanBeEnteredAndTicked()
        {
            var machine = TrafficLight();
            machine.Start(Light.Red);
            machine.Go(Light.Off);

            Assert.DoesNotThrow(() => machine.Tick(0.016f));
        }

        [Test]
        public void Allow_UnknownState_Throws()
        {
            var machine = new StateMachine<Light>().AddState(Light.Red);

            Assert.Throws<InvalidOperationException>(() => machine.Allow(Light.Red, Light.Green));
        }

        [Test]
        public void Start_Twice_Throws()
        {
            var machine = TrafficLight();
            machine.Start(Light.Red);

            Assert.Throws<InvalidOperationException>(() => machine.Start(Light.Green));
        }

        [Test]
        public void ChangingStateAndTicking_AllocatesNothing()
        {
            var machine = new StateMachine<Light>()
                .AddState(Light.Red, new Idle())
                .AddState(Light.Green, new Idle())
                .Allow(Light.Red, Light.Green)
                .Allow(Light.Green, Light.Red);
            machine.Start(Light.Red);
            machine.TryGo(Light.Green);
            machine.TryGo(Light.Red);

            Assert.That(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    machine.TryGo(Light.Green);
                    machine.Tick(0.016f);
                    machine.Go(Light.Red);
                    machine.IsIn(Light.Red);
                }
            }, Is.Not.AllocatingGCMemory());
        }

        private sealed class Idle : State
        {
        }
    }
}
