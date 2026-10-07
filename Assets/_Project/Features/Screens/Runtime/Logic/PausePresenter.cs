using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Flow;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>The pause panel: carry on, or give the run up and go back to the title screen.</summary>
    internal sealed class PausePresenter : IStartable, IDisposable
    {
        private readonly IPauseView _view;
        private readonly IGameFlow _flow;

        public PausePresenter(IPauseView view, IGameFlow flow)
        {
            _view = Guard.NotNull(view, nameof(view));
            _flow = Guard.NotNull(flow, nameof(flow));
        }

        public void Start()
        {
            _view.ResumePressed += OnResumePressed;
            _view.QuitPressed += OnQuitPressed;
        }

        public void Dispose()
        {
            _view.ResumePressed -= OnResumePressed;
            _view.QuitPressed -= OnQuitPressed;
        }

        private void OnResumePressed() => _flow.Resume();

        private void OnQuitPressed() => _flow.ReturnToMenu();
    }
}
