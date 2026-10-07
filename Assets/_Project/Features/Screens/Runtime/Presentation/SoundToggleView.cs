using System;
using System.Collections.Generic;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// Every button marked "sound-toggle" is the same control, wherever it appears. Each holds
    /// an "on" label and an "off" label, and the style sheet shows the one that applies.
    /// </summary>
    internal sealed class SoundToggleView : ISoundToggleView, IDocumentView
    {
        private const string ToggleClass = "sound-toggle";
        private const string OffClass = "sound-toggle--off";

        private readonly List<Button> _toggles = new List<Button>();

        public event Action Pressed;

        public void Bind(VisualElement documentRoot)
        {
            documentRoot.Query<Button>(className: ToggleClass).ToList(_toggles);
            if (_toggles.Count == 0) throw new InvalidOperationException("The UI document has no Button marked '" + ToggleClass + "'.");

            foreach (Button toggle in _toggles) toggle.clicked += OnClicked;
        }

        public void Unbind()
        {
            foreach (Button toggle in _toggles) toggle.clicked -= OnClicked;
            _toggles.Clear();
        }

        public void SetSoundOn(bool on)
        {
            foreach (Button toggle in _toggles) toggle.EnableInClassList(OffClass, !on);
        }

        private void OnClicked() => Pressed?.Invoke();
    }
}
