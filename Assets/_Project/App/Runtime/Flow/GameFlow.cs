using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.StateMachines;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>
    /// Owns the game's phase. The transition table below is the whole flow; anything not
    /// listed there cannot happen, whoever asks for it.
    /// </summary>
    internal sealed class GameFlow : IGameFlow
    {
        private readonly StateMachine<GamePhase> _machine;
        private readonly IPublisher _publisher;
        private readonly IRunSeedSource _seeds;

        public GameFlow(IPublisher publisher, IRunSeedSource seeds)
        {
            _publisher = Guard.NotNull(publisher, nameof(publisher));
            _seeds = Guard.NotNull(seeds, nameof(seeds));

            _machine = new StateMachine<GamePhase>()
                .AddState(GamePhase.Boot)
                .AddState(GamePhase.Menu)
                .AddState(GamePhase.Run)
                .AddState(GamePhase.Paused)
                .AddState(GamePhase.GameOver)
                .Allow(GamePhase.Boot, GamePhase.Menu)
                .Allow(GamePhase.Menu, GamePhase.Run)
                .Allow(GamePhase.Run, GamePhase.Paused)
                .Allow(GamePhase.Paused, GamePhase.Run)
                .Allow(GamePhase.Paused, GamePhase.Menu)
                .Allow(GamePhase.Run, GamePhase.GameOver)
                .Allow(GamePhase.GameOver, GamePhase.Run)
                .Allow(GamePhase.GameOver, GamePhase.Menu);

            _machine.Changed += OnPhaseChanged;
        }

        public GamePhase Phase => _machine.Current;

        public void Start() => _machine.Start(GamePhase.Boot);

        public void FinishBoot() => _machine.Go(GamePhase.Menu);

        public bool StartRun()
        {
            // Paused -> Run is a resume, not a new run.
            if (_machine.IsIn(GamePhase.Paused) || !_machine.CanGo(GamePhase.Run)) return false;

            // Announced before the phase changes, so systems have reset by the time they first tick.
            _publisher.Publish(new RunStarted(_seeds.NextSeed()));
            return _machine.TryGo(GamePhase.Run);
        }

        public bool Pause() => _machine.IsIn(GamePhase.Run) && _machine.TryGo(GamePhase.Paused);

        public bool Resume() => _machine.IsIn(GamePhase.Paused) && _machine.TryGo(GamePhase.Run);

        public bool FailRun()
        {
            if (!_machine.IsIn(GamePhase.Run)) return false;

            _machine.Go(GamePhase.GameOver);
            _publisher.Publish(new RunEnded(RunEndReason.Crashed));
            return true;
        }

        public bool ReturnToMenu()
        {
            bool abandoning = _machine.IsIn(GamePhase.Paused);
            if (!_machine.TryGo(GamePhase.Menu)) return false;

            if (abandoning) _publisher.Publish(new RunEnded(RunEndReason.Abandoned));
            return true;
        }

        private void OnPhaseChanged(GamePhase previous, GamePhase current)
        {
            _publisher.Publish(new GamePhaseChanged(previous, current));
        }
    }
}
