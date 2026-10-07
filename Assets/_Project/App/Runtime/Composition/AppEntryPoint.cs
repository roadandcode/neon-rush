using RoadAndCode.Core.Simulation;
using RoadAndCode.NeonRush.App.Flow;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>Bridges Unity's player loop to the game: boots the flow and drives the simulation.</summary>
    internal sealed class AppEntryPoint : IStartable, ITickable
    {
        // A browser tab coming back from the background can report seconds of delta time.
        // Simulating that in one step would teleport everything through everything else.
        private const float MaxStep = 1f / 20f;

        private readonly GameFlow _flow;
        private readonly SimulationLoop _simulation;

        public AppEntryPoint(GameFlow flow, SimulationLoop simulation)
        {
            _flow = flow;
            _simulation = simulation;
        }

        public void Start()
        {
            _flow.Start();
            _flow.FinishBoot();
        }

        public void Tick()
        {
            _simulation.Tick(Mathf.Min(Time.deltaTime, MaxStep));
        }
    }
}
