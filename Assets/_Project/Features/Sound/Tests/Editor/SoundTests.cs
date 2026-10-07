using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.Core.Persistence;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Scoring;
using RoadAndCode.NeonRush.Shared.Screens;
using RoadAndCode.NeonRush.Shared.Sound;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Sound.Data;
using RoadAndCode.NeonRush.Sound.Logic;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Sound.Tests
{
    internal sealed class FakeSoundPlayer : ISoundPlayer
    {
        public List<(SoundCue Cue, float Pitch, float Delay)> Played { get; } = new List<(SoundCue, float, float)>();

        public void Play(SoundCue cue, float pitch = 1f, float delay = 0f) => Played.Add((cue, pitch, delay));
    }

    internal sealed class FakeMusicPlayer : IMusicPlayer
    {
        public float Menu { get; private set; }

        public float Run { get; private set; }

        public int Calls { get; private set; }

        public void SetLevels(float menu, float run)
        {
            Menu = menu;
            Run = run;
            Calls++;
        }
    }

    internal sealed class FakeMasterVolume : IMasterVolume
    {
        public bool? Muted { get; private set; }

        public void SetMuted(bool muted) => Muted = muted;
    }

    internal sealed class StubFlow : IGameFlow
    {
        public GamePhase Phase { get; set; } = GamePhase.Boot;

        public bool StartRun() => false;

        public bool Pause() => false;

        public bool Resume() => false;

        public bool FailRun() => false;

        public bool ReturnToMenu() => false;
    }

    public sealed class SoundDirectorTests
    {
        private const float Tolerance = 0.0001f;

        private FakeSoundPlayer _player;
        private SoundMix _mix;
        private MessageBus _bus;
        private SoundDirector _director;

        [SetUp]
        public void SetUp()
        {
            _player = new FakeSoundPlayer();
            _mix = new SoundMix();
            _bus = new MessageBus();
            _director = new SoundDirector(_player, _mix, _bus);
            _director.Start();
        }

        private SoundCue[] Cues()
        {
            var cues = new SoundCue[_player.Played.Count];
            for (int i = 0; i < cues.Length; i++) cues[i] = _player.Played[i].Cue;
            return cues;
        }

        [Test]
        public void EachThingThatHappens_HasItsOwnSound()
        {
            _bus.Publish(new ButtonPressed());
            _bus.Publish(new RunIntroStarted(1.5f));
            _bus.Publish(new RunnerMoved(RunnerMove.ChangedLane));
            _bus.Publish(new RunnerMoved(RunnerMove.Jumped));
            _bus.Publish(new RunnerMoved(RunnerMove.Slid));
            _bus.Publish(new PickupCollected(10));
            _bus.Publish(new HazardHit());

            Assert.That(Cues(), Is.EqualTo(new[]
            {
                SoundCue.ButtonPress, SoundCue.IntroSwoosh, SoundCue.LaneChange, SoundCue.Jump,
                SoundCue.Slide, SoundCue.Pickup, SoundCue.Crash,
            }));
        }

        [Test]
        public void APickupOutsideACombo_PlaysAtItsNaturalPitch()
        {
            _bus.Publish(new ScoreChanged(score: 10, multiplier: 1));

            _bus.Publish(new PickupCollected(10));

            Assert.That(_player.Played[0].Pitch, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void EachStepOfACombo_RaisesThePickupByTheSameInterval()
        {
            _bus.Publish(new ScoreChanged(score: 10, multiplier: 2));
            _bus.Publish(new PickupCollected(10));
            _bus.Publish(new ScoreChanged(score: 40, multiplier: 3));
            _bus.Publish(new PickupCollected(10));

            float first = _player.Played[0].Pitch;
            float second = _player.Played[1].Pitch;

            Assert.That(first, Is.GreaterThan(1f));
            Assert.That(second / first, Is.EqualTo(first).Within(Tolerance), "Equal musical steps are equal ratios.");
        }

        [Test]
        public void ThePickupPitch_StopsClimbing()
        {
            _bus.Publish(new ScoreChanged(score: 10, multiplier: 1 + _mix.PickupMaxSteps));
            _bus.Publish(new PickupCollected(10));
            _bus.Publish(new ScoreChanged(score: 20, multiplier: 1 + _mix.PickupMaxSteps + 5));
            _bus.Publish(new PickupCollected(10));

            Assert.That(_player.Played[1].Pitch, Is.EqualTo(_player.Played[0].Pitch).Within(Tolerance));
        }

        [Test]
        public void ANewBest_IsCelebratedAfterTheCrashHasSounded()
        {
            _bus.Publish(new RunScored(score: 500, best: 500, isNewBest: true));

            Assert.That(_player.Played, Has.Count.EqualTo(1));
            Assert.That(_player.Played[0].Cue, Is.EqualTo(SoundCue.NewBest));
            Assert.That(_player.Played[0].Delay, Is.EqualTo(_mix.NewBestDelay));
        }

        [Test]
        public void ARunThatDoesNotBeatTheBest_GetsNoFanfare()
        {
            _bus.Publish(new RunScored(score: 100, best: 500, isNewBest: false));

            Assert.That(_player.Played, Is.Empty);
        }

        [Test]
        public void AFirstRunThatScoresNothing_GetsNoFanfare()
        {
            _bus.Publish(new RunScored(score: 0, best: 0, isNewBest: false));

            Assert.That(_player.Played, Is.Empty);
        }

        [Test]
        public void AfterDispose_NothingPlays()
        {
            _director.Dispose();

            _bus.Publish(new HazardHit());
            _bus.Publish(new PickupCollected(10));

            Assert.That(_player.Played, Is.Empty);
        }
    }

    public sealed class MusicDirectorTests
    {
        private const float Tolerance = 0.0001f;

        private FakeMusicPlayer _player;
        private SoundMix _mix;
        private StubFlow _flow;
        private MessageBus _bus;
        private MusicDirector _director;

        [SetUp]
        public void SetUp()
        {
            _player = new FakeMusicPlayer();
            _mix = new SoundMix();
            _flow = new StubFlow { Phase = GamePhase.Menu };
            _bus = new MessageBus();
            _director = new MusicDirector(_player, _mix, _flow, _bus);
            _director.Start();
        }

        private void Enter(GamePhase phase)
        {
            GamePhase previous = _flow.Phase;
            _flow.Phase = phase;
            _bus.Publish(new GamePhaseChanged(previous, phase));
        }

        private void Settle() => _director.Advance(_mix.MusicFadeSeconds * 2f);

        [Test]
        public void OnTheMenu_OnlyTheMenuLoopIsHeard()
        {
            Settle();

            Assert.That(_player.Menu, Is.EqualTo(1f));
            Assert.That(_player.Run, Is.Zero);
        }

        [TestCase(GamePhase.Intro)]
        [TestCase(GamePhase.Run)]
        public void FromTheIntroOnwards_TheRunLoopTakesOver(GamePhase phase)
        {
            Settle();

            Enter(phase);
            Settle();

            Assert.That(_player.Menu, Is.Zero);
            Assert.That(_player.Run, Is.EqualTo(1f));
        }

        [Test]
        public void TheChange_IsAFade_NotACut()
        {
            Settle();
            Enter(GamePhase.Run);

            _director.Advance(_mix.MusicFadeSeconds * 0.5f);

            Assert.That(_player.Menu, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(_player.Run, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void Pausing_DucksTheMusic_AndResumingBringsItBack()
        {
            Enter(GamePhase.Run);
            Settle();

            Enter(GamePhase.Paused);
            Settle();
            Assert.That(_player.Run, Is.EqualTo(_mix.PausedMusicLevel).Within(Tolerance));

            Enter(GamePhase.Run);
            Settle();
            Assert.That(_player.Run, Is.EqualTo(1f));
        }

        [Test]
        public void OnGameOver_TheMusicDropsBack()
        {
            Enter(GamePhase.Run);
            Settle();

            Enter(GamePhase.GameOver);
            Settle();

            Assert.That(_player.Run, Is.EqualTo(_mix.GameOverMusicLevel).Within(Tolerance));
            Assert.That(_player.Menu, Is.Zero);
        }

        [Test]
        public void OnceSettled_ThePlayerIsLeftAlone()
        {
            Settle();
            int calls = _player.Calls;

            for (int i = 0; i < 100; i++) _director.Advance(0.016f);

            Assert.That(_player.Calls, Is.EqualTo(calls));
        }

        [Test]
        public void Fading_AllocatesNothing()
        {
            Settle();

            Assert.That(() =>
            {
                Enter(GamePhase.Run);
                for (int i = 0; i < 200; i++) _director.Advance(0.016f);
                Enter(GamePhase.Paused);
                for (int i = 0; i < 200; i++) _director.Advance(0.016f);
            }, Is.Not.AllocatingGCMemory());
        }
    }

    public sealed class SoundSettingsTests
    {
        private InMemorySaveStore _store;
        private FakeMasterVolume _volume;
        private MessageBus _bus;
        private List<bool> _announced;

        [SetUp]
        public void SetUp()
        {
            _store = new InMemorySaveStore();
            _volume = new FakeMasterVolume();
            _bus = new MessageBus();
            _announced = new List<bool>();
            _bus.Subscribe<SoundSettingChanged>(m => _announced.Add(m.SoundOn));
        }

        private SoundSettings Create()
        {
            var settings = new SoundSettings(_store, _volume, _bus);
            settings.Start();
            return settings;
        }

        [Test]
        public void WithNothingSaved_SoundIsOn()
        {
            ISoundSettings settings = Create();

            Assert.That(settings.SoundOn, Is.True);
            Assert.That(_volume.Muted, Is.False);
        }

        [Test]
        public void TurningSoundOff_MutesAtOnce_AndSaysSo()
        {
            ISoundSettings settings = Create();

            settings.SetSoundOn(false);

            Assert.That(settings.SoundOn, Is.False);
            Assert.That(_volume.Muted, Is.True);
            Assert.That(_announced, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void TheChoice_IsRememberedNextTime()
        {
            Create().SetSoundOn(false);

            ISoundSettings later = Create();

            Assert.That(later.SoundOn, Is.False);
            Assert.That(_volume.Muted, Is.True);
        }

        [Test]
        public void SettingItToWhatItAlreadyIs_DoesNothing()
        {
            ISoundSettings settings = Create();

            settings.SetSoundOn(true);

            Assert.That(_announced, Is.Empty);
        }
    }
}
