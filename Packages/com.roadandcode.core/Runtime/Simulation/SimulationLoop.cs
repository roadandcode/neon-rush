using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;

namespace RoadAndCode.Core.Simulation
{
    /// <summary>A piece of gameplay logic that advances with simulated time.</summary>
    public interface ISimulationSystem
    {
        void Tick(float deltaTime);
    }

    /// <summary>
    /// Ticks gameplay systems in a fixed, explicit order: the order they were given in.
    /// Pausing the game is switching this off; nothing else needs to know about it.
    /// </summary>
    public sealed class SimulationLoop
    {
        private readonly ISimulationSystem[] _systems;

        public SimulationLoop(IEnumerable<ISimulationSystem> systems)
        {
            Guard.NotNull(systems, nameof(systems));
            _systems = new List<ISimulationSystem>(systems).ToArray();
        }

        public bool IsRunning { get; set; }

        public int SystemCount => _systems.Length;

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;
            for (int i = 0; i < _systems.Length; i++) _systems[i].Tick(deltaTime);
        }
    }
}
