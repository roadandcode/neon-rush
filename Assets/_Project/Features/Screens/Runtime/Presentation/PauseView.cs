using System;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    internal sealed class PauseView : IPauseView, IDocumentView
    {
        private const string RootName = "pause";
        private const string ResumeButtonName = "pause-resume";
        private const string QuitButtonName = "pause-quit";

        private readonly ButtonRing _ring = new ButtonRing();
        private VisualElement _root;
        private Button _resume;
        private Button _quit;

        public event Action ResumePressed;

        public event Action QuitPressed;

        public void Bind(VisualElement documentRoot)
        {
            _root = documentRoot.Require<VisualElement>(RootName);
            _resume = _root.Require<Button>(ResumeButtonName);
            _quit = _root.Require<Button>(QuitButtonName);

            _resume.clicked += OnResumeClicked;
            _quit.clicked += OnQuitClicked;
            _ring.Collect(_root);
        }

        public void Unbind()
        {
            if (_resume != null) _resume.clicked -= OnResumeClicked;
            if (_quit != null) _quit.clicked -= OnQuitClicked;

            _ring.Clear();
            _root = null;
            _resume = null;
            _quit = null;
        }

        public int ControlCount => _ring.Count;

        public void SetVisible(bool visible) => _root.SetShown(visible);

        public void SetFocus(int index) => _ring.SetFocus(index);

        public void Press(int index) => _ring.Press(index);

        private void OnResumeClicked() => ResumePressed?.Invoke();

        private void OnQuitClicked() => QuitPressed?.Invoke();
    }
}
