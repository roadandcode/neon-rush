using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Scoring;
using RoadAndCode.NeonRush.Shared.Screens;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Sound.Data;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Sound.Logic
{
    /// <summary>
    /// Turns what happens in the game into sound cues. No other feature knows there is audio:
    /// they announce what happened, as they did before this existed, and this listens.
    /// </summary>
    internal sealed class SoundDirector : IStartable, IDisposable
    {
        private const float SemitonesPerOctave = 12f;
        private const int RestingMultiplier = 1;

        private readonly ISoundPlayer _player;
        private readonly SoundMix _mix;
        private readonly ISubscriber _subscriber;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private int _multiplier = RestingMultiplier;

        public SoundDirector(ISoundPlayer player, SoundMix mix, ISubscriber subscriber)
        {
            _player = Guard.NotNull(player, nameof(player));
            _mix = Guard.NotNull(mix, nameof(mix));
            _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
        }

        public void Start()
        {
            _subscriber.Subscribe<ButtonPressed>(OnButtonPressed).AddTo(_subscriptions);
            _subscriber.Subscribe<RunIntroStarted>(OnIntroStarted).AddTo(_subscriptions);
            _subscriber.Subscribe<RunnerMoved>(OnRunnerMoved).AddTo(_subscriptions);
            _subscriber.Subscribe<ScoreChanged>(OnScoreChanged).AddTo(_subscriptions);
            _subscriber.Subscribe<PickupCollected>(OnPickupCollected).AddTo(_subscriptions);
            _subscriber.Subscribe<HazardHit>(OnHazardHit).AddTo(_subscriptions);
            _subscriber.Subscribe<RunScored>(OnRunScored).AddTo(_subscriptions);
        }

        public void Dispose() => _subscriptions.Dispose();

        private void OnButtonPressed(ButtonPressed message) => _player.Play(SoundCue.ButtonPress);

        private void OnIntroStarted(RunIntroStarted message) => _player.Play(SoundCue.IntroSwoosh);

        private void OnRunnerMoved(RunnerMoved message)
        {
            switch (message.Move)
            {
                case RunnerMove.Jumped: _player.Play(SoundCue.Jump); break;
                case RunnerMove.Slid: _player.Play(SoundCue.Slide); break;
                default: _player.Play(SoundCue.LaneChange); break;
            }
        }

        private void OnScoreChanged(ScoreChanged message) => _multiplier = message.Multiplier;

        // A streak climbs in pitch, which is how the player hears the combo without looking at it.
        private void OnPickupCollected(PickupCollected message)
        {
            int steps = Mathf.Clamp(_multiplier - RestingMultiplier, 0, _mix.PickupMaxSteps);
            float pitch = Mathf.Pow(2f, steps * _mix.PickupSemitonesPerCombo / SemitonesPerOctave);
            _player.Play(SoundCue.Pickup, pitch);
        }

        private void OnHazardHit(HazardHit message) => _player.Play(SoundCue.Crash);

        // A first run that scores nothing "beats" a best of zero. That is not worth a fanfare.
        private void OnRunScored(RunScored message)
        {
            if (message.IsNewBest && message.Score > 0) _player.Play(SoundCue.NewBest, delay: _mix.NewBestDelay);
        }
    }
}
