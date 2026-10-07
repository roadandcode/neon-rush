using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Platforms;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Logic
{
    /// <summary>
    /// Keeps the UI's content clear of notches and cut-outs. The safe area has no change event:
    /// turning a phone from one landscape to the other moves the notch to the opposite side
    /// without resizing anything, so the insets are compared every frame and applied only when
    /// they differ.
    /// </summary>
    internal sealed class SafeAreaPresenter : IStartable, ITickable, IDisposable
    {
        private readonly ISafeAreaView _view;
        private readonly ISafeArea _safeArea;
        private ScreenInsets _applied;

        public SafeAreaPresenter(ISafeAreaView view, ISafeArea safeArea)
        {
            _view = Guard.NotNull(view, nameof(view));
            _safeArea = Guard.NotNull(safeArea, nameof(safeArea));
        }

        public void Start()
        {
            _view.Resized += Apply;
            Apply();
        }

        public void Tick()
        {
            if (!_safeArea.Insets.Equals(_applied)) Apply();
        }

        public void Dispose() => _view.Resized -= Apply;

        private void Apply()
        {
            _applied = _safeArea.Insets;
            _view.SetInsets(_applied);
        }
    }
}
