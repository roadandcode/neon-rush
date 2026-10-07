using System;
using System.Collections.Generic;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Input;
using RoadAndCode.NeonRush.Shared.Scoring;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    /// <summary>Records what was asked of the flow and always agrees.</summary>
    internal sealed class FakeFlow : IGameFlow
    {
        public GamePhase Phase { get; set; } = GamePhase.Menu;

        public int RunsStarted { get; private set; }

        public int Pauses { get; private set; }

        public int Resumes { get; private set; }

        public int ReturnsToMenu { get; private set; }

        public bool StartRun()
        {
            RunsStarted++;
            return true;
        }

        public bool Pause()
        {
            Pauses++;
            return true;
        }

        public bool Resume()
        {
            Resumes++;
            return true;
        }

        public bool FailRun() => false;

        public bool ReturnToMenu()
        {
            ReturnsToMenu++;
            return true;
        }
    }

    internal sealed class FakeBestScore : IBestScore
    {
        public int Best { get; set; }
    }

    internal class FakeScreen : IScreen
    {
        /// <summary>Null until something shows or hides the screen.</summary>
        public bool? Visible { get; private set; }

        public void SetVisible(bool visible) => Visible = visible;
    }

    internal sealed class FakeMenuView : FakeScreen, IMenuView
    {
        public event Action PlayPressed;

        public int? Best { get; private set; }

        public void SetBest(int best) => Best = best;

        public void PressPlay() => PlayPressed?.Invoke();
    }

    internal sealed class FakeHudView : FakeScreen, IHudView
    {
        public event Action PausePressed;

        public int? Score { get; private set; }

        public int? Multiplier { get; private set; }

        public bool ComboActive { get; private set; }

        public void SetScore(int score) => Score = score;

        public void SetMultiplier(int multiplier, bool comboActive)
        {
            Multiplier = multiplier;
            ComboActive = comboActive;
        }

        public void PressPause() => PausePressed?.Invoke();
    }

    internal sealed class FakePauseView : FakeScreen, IPauseView
    {
        public event Action ResumePressed;

        public event Action QuitPressed;

        public void PressResume() => ResumePressed?.Invoke();

        public void PressQuit() => QuitPressed?.Invoke();
    }

    internal sealed class FakeGameOverView : FakeScreen, IGameOverView
    {
        public event Action RetryPressed;

        public event Action MenuPressed;

        public (int Score, int Best, bool IsNewBest)? Result { get; private set; }

        public void ShowResult(int score, int best, bool isNewBest) => Result = (score, best, isNewBest);

        public void PressRetry() => RetryPressed?.Invoke();

        public void PressMenu() => MenuPressed?.Invoke();
    }

    internal sealed class FakeControlHintsView : IControlHintsView
    {
        public List<string> Shown { get; } = new List<string>();

        public void ShowHintsFor(string controlScheme) => Shown.Add(controlScheme);
    }

    internal sealed class FakeSafeAreaView : ISafeAreaView
    {
        public event Action Resized;

        public List<ScreenInsets> Applied { get; } = new List<ScreenInsets>();

        public void SetInsets(ScreenInsets insets) => Applied.Add(insets);

        public void Resize() => Resized?.Invoke();
    }

    internal sealed class FakeSafeArea : ISafeArea
    {
        public ScreenInsets Insets { get; set; }
    }

    internal sealed class FakeInputProfile : IInputProfile
    {
        private readonly string[] _schemes;

        public FakeInputProfile(params string[] schemes)
        {
            _schemes = schemes;
        }

        public IReadOnlyList<string> ControlSchemes => _schemes;

        public bool Uses(string controlScheme) => Array.IndexOf(_schemes, controlScheme) >= 0;
    }
}
