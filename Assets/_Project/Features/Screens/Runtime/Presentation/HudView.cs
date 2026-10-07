using System;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    internal sealed class HudView : IHudView, IDocumentView
    {
        private const string RootName = "hud";
        private const string PauseButtonName = "hud-pause";
        private const string ScoreDigitsName = "hud-score";
        private const string MultiplierName = "hud-multiplier";
        private const string MultiplierDigitsName = "hud-multiplier-value";
        private const string ComboClass = "multiplier--combo";

        private VisualElement _root;
        private Button _pause;
        private VisualElement _multiplierBadge;
        private DigitStrip _score;
        private DigitStrip _multiplier;

        public event Action PausePressed;

        public void Bind(VisualElement documentRoot)
        {
            _root = documentRoot.Require<VisualElement>(RootName);
            _pause = _root.Require<Button>(PauseButtonName);
            _multiplierBadge = _root.Require<VisualElement>(MultiplierName);
            _score = new DigitStrip(_root.Require<VisualElement>(ScoreDigitsName), ScoreDisplay.Digits);
            _multiplier = new DigitStrip(_root.Require<VisualElement>(MultiplierDigitsName), ScoreDisplay.MultiplierDigits);

            _pause.clicked += OnPauseClicked;
        }

        public void Unbind()
        {
            if (_pause != null) _pause.clicked -= OnPauseClicked;

            _root = null;
            _pause = null;
            _multiplierBadge = null;
            _score = null;
            _multiplier = null;
        }

        public void SetVisible(bool visible) => _root.SetShown(visible);

        public void SetScore(int score) => _score.Show(score);

        public void SetMultiplier(int multiplier, bool comboActive)
        {
            _multiplier.Show(multiplier);
            _multiplierBadge.EnableInClassList(ComboClass, comboActive);
        }

        private void OnPauseClicked() => PausePressed?.Invoke();
    }
}
