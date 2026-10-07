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
        private readonly FlowSettings _settings;

        public GameFlow(IPublisher publisher, IRunSeedSource seeds, FlowSettings settings)
        {
            _publisher = Guard.NotNull(publisher, nameof(publisher));
            _seeds = Guard.NotNull(seeds, nameof(seeds));
            _settings = Guard.NotNull(settings, nameof(settings));

            _machine = new StateMachine<GamePhase>()
                .AddState(GamePhase.Boot)
                .AddState(GamePhase.Menu)
                .AddState(GamePhase.Intro)
                .AddState(GamePhase.Run)
                .AddState(GamePhase.Paused)
                .AddState(GamePhase.GameOver)
                .Allow(GamePhase.Boot, GamePhase.Menu)
                .Allow(GamePhase.Menu, GamePhase.Intro)
                .Allow(GamePhase.Menu, GamePhase.Run)
                .Allow(GamePhase.Intro, GamePhase.Run)
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
            bool fromMenu = _machine.IsIn(GamePhase.Menu);
            if (!fromMenu && !_machine.IsIn(GamePhase.GameOver)) return false;

            // Announced before the phase changes, so systems have reset by the time anything is shown or ticked.
            _publisher.Publish(new RunStarted(_seeds.NextSeed()));

            if (fromMenu && _settings.IntroSeconds > 0f)
            {
                _machine.Go(GamePhase.Intro);
                _publisher.Publish(new RunIntroStarted(_settings.IntroSeconds));
                return true;
            }

            _machine.Go(GamePhase.Run);
            return true;
        }

        /// <summary>The intro has played out. Called by whoever is keeping its time.</summary>
        public bool FinishIntro() => _machine.IsIn(GamePhase.Intro) && _machine.TryGo(GamePhase.Run);

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

            // After the result is recorded: the score keeper still needs the run it is about to lose.
            _publisher.Publish(new StageCleared());
            return true;
        }

        private void OnPhaseChanged(GamePhase previous, GamePhase current)
        {
            _publisher.Publish(new GamePhaseChanged(previous, current));
        }
    }
}
