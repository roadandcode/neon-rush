using System;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    internal sealed class GameOverView : IGameOverView, IDocumentView
    {
        private const string RootName = "game-over";
        private const string RetryButtonName = "game-over-retry";
        private const string MenuButtonName = "game-over-menu";
        private const string ScoreDigitsName = "game-over-score";
        private const string BestDigitsName = "game-over-best";
        private const string NewBestName = "game-over-new-best";

        private VisualElement _root;
        private Button _retry;
        private Button _menu;
        private VisualElement _newBest;
        private DigitStrip _score;
        private DigitStrip _best;

        public event Action RetryPressed;

        public event Action MenuPressed;

        public void Bind(VisualElement documentRoot)
        {
            _root = documentRoot.Require<VisualElement>(RootName);
            _retry = _root.Require<Button>(RetryButtonName);
            _menu = _root.Require<Button>(MenuButtonName);
            _newBest = _root.Require<VisualElement>(NewBestName);
            _score = new DigitStrip(_root.Require<VisualElement>(ScoreDigitsName), ScoreDisplay.Digits);
            _best = new DigitStrip(_root.Require<VisualElement>(BestDigitsName), ScoreDisplay.Digits);

            _retry.clicked += OnRetryClicked;
            _menu.clicked += OnMenuClicked;
        }

        public void Unbind()
        {
            if (_retry != null) _retry.clicked -= OnRetryClicked;
            if (_menu != null) _menu.clicked -= OnMenuClicked;

            _root = null;
            _retry = null;
            _menu = null;
            _newBest = null;
            _score = null;
            _best = null;
        }

        public void SetVisible(bool visible) => _root.SetShown(visible);

        public void ShowResult(int score, int best, bool isNewBest)
        {
            _score.Show(score);
            _best.Show(best);
            _newBest.SetShown(isNewBest);
        }

        private void OnRetryClicked() => RetryPressed?.Invoke();

        private void OnMenuClicked() => MenuPressed?.Invoke();
    }
}
