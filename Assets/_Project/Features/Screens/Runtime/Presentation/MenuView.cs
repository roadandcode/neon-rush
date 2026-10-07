using System;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    internal sealed class MenuView : IMenuView, IDocumentView
    {
        private const string RootName = "menu";
        private const string PlayButtonName = "menu-play";
        private const string BestDigitsName = "menu-best";

        private readonly ButtonRing _ring = new ButtonRing();
        private VisualElement _root;
        private Button _play;
        private DigitStrip _best;

        public event Action PlayPressed;

        public void Bind(VisualElement documentRoot)
        {
            _root = documentRoot.Require<VisualElement>(RootName);
            _play = _root.Require<Button>(PlayButtonName);
            _best = new DigitStrip(_root.Require<VisualElement>(BestDigitsName), ScoreDisplay.Digits);

            _play.clicked += OnPlayClicked;
            _ring.Collect(_root);
        }

        public void Unbind()
        {
            if (_play != null) _play.clicked -= OnPlayClicked;

            _ring.Clear();
            _root = null;
            _play = null;
            _best = null;
        }

        public int ControlCount => _ring.Count;

        public void SetVisible(bool visible) => _root.SetShown(visible);

        public void SetFocus(int index) => _ring.SetFocus(index);

        public void Press(int index) => _ring.Press(index);

        public void SetBest(int best) => _best.Show(best);

        private void OnPlayClicked() => PlayPressed?.Invoke();
    }
}
