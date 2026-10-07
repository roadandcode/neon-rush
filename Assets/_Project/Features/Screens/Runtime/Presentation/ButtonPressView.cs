using System;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>Hears every button in the document through one listener on its root, including buttons added later.</summary>
    internal sealed class ButtonPressView : IButtonPressView, IDocumentView
    {
        private VisualElement _root;

        public event Action ButtonPressed;

        public void Bind(VisualElement documentRoot)
        {
            _root = documentRoot;
            _root.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
        }

        public void Unbind()
        {
            _root?.UnregisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            _root = null;
        }

        private void OnClick(ClickEvent click)
        {
            if (click.target is Button) ButtonPressed?.Invoke();
        }
    }
}
