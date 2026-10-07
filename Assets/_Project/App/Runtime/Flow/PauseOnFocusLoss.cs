using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Shared.Flow;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>
    /// Pauses a run the moment the player stops looking at the game: a phone call, another tab,
    /// another window. Coming back never resumes by itself; the player does that when ready.
    /// </summary>
    internal sealed class PauseOnFocusLoss : IStartable, IDisposable
    {
        private readonly IAppFocus _focus;
        private readonly IGameFlow _flow;

        public PauseOnFocusLoss(IAppFocus focus, IGameFlow flow)
        {
            _focus = Guard.NotNull(focus, nameof(focus));
            _flow = Guard.NotNull(flow, nameof(flow));
        }

        public void Start() => _focus.Changed += OnFocusChanged;

        public void Dispose() => _focus.Changed -= OnFocusChanged;

        // Pause refuses unless a run is in progress, so no phase check is needed here.
        private void OnFocusChanged(bool focused)
        {
            if (!focused) _flow.Pause();
        }
    }
}
