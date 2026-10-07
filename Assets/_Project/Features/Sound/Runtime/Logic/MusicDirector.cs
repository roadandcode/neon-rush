using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Sound.Data;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Sound.Logic
{
    /// <summary>
    /// Chooses the music level for each phase and fades towards it. The menu loop and the run
    /// loop play in step the whole time, so starting a run is the drums and bass fading in over
    /// music that was already playing, not one track stopping and another starting.
    /// </summary>
    internal sealed class MusicDirector : IStartable, ITickable, IDisposable
    {
        private readonly IMusicPlayer _player;
        private readonly SoundMix _mix;
        private readonly IGameFlow _flow;
        private readonly ISubscriber _subscriber;
        private IDisposable _subscription;

        private float _menu;
        private float _run;
        private float _menuTarget;
        private float _runTarget;

        public MusicDirector(IMusicPlayer player, SoundMix mix, IGameFlow flow, ISubscriber subscriber)
        {
            _player = Guard.NotNull(player, nameof(player));
            _mix = Guard.NotNull(mix, nameof(mix));
            _flow = Guard.NotNull(flow, nameof(flow));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            Aim(_flow.Phase);
            _subscription = _subscriber.Subscribe<GamePhaseChanged>(OnPhaseChanged);
        }

        public void Tick() => Advance(Time.deltaTime);

        public void Dispose() => _subscription?.Dispose();

        /// <summary>Moves the levels towards their targets. Does nothing once they are there.</summary>
        internal void Advance(float deltaTime)
        {
            if (_menu == _menuTarget && _run == _runTarget) return;

            float step = deltaTime / _mix.MusicFadeSeconds;
            _menu = Mathf.MoveTowards(_menu, _menuTarget, step);
            _run = Mathf.MoveTowards(_run, _runTarget, step);
            _player.SetLevels(_menu, _run);
        }

        private void OnPhaseChanged(GamePhaseChanged message) => Aim(message.Current);

        private void Aim(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Menu:
                    _menuTarget = 1f;
                    _runTarget = 0f;
                    break;
                case GamePhase.Intro:
                case GamePhase.Run:
                    _menuTarget = 0f;
                    _runTarget = 1f;
                    break;
                case GamePhase.Paused:
                    _menuTarget = 0f;
                    _runTarget = _mix.PausedMusicLevel;
                    break;
                case GamePhase.GameOver:
                    _menuTarget = 0f;
                    _runTarget = _mix.GameOverMusicLevel;
                    break;
                default:
                    _menuTarget = 0f;
                    _runTarget = 0f;
                    break;
            }
        }
    }
}
