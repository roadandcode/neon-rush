using System;
using System.Collections.Generic;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// Pads every element marked "safe-area" so its content stays clear of notches and cut-outs.
    /// Backdrops are not marked, so they still run to the edge of the glass.
    /// </summary>
    internal sealed class SafeAreaView : ISafeAreaView, IDocumentView
    {
        private const string SafeAreaClass = "safe-area";

        private readonly List<VisualElement> _padded = new List<VisualElement>();
        private VisualElement _root;

        public event Action Resized;

        public void Bind(VisualElement documentRoot)
        {
            _root = documentRoot;
            _root.Query(className: SafeAreaClass).ToList(_padded);
            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void Unbind()
        {
            _root?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _padded.Clear();
            _root = null;
        }

        public void SetInsets(ScreenInsets insets)
        {
            // The root fills the panel, so its size is the screen measured in the UI's own units.
            // Before the first layout pass there is no size yet; the resize that follows brings us back here.
            float width = _root.layout.width;
            float height = _root.layout.height;
            if (float.IsNaN(width) || float.IsNaN(height)) return;

            for (int i = 0; i < _padded.Count; i++)
            {
                IStyle style = _padded[i].style;
                style.paddingLeft = insets.Left * width;
                style.paddingRight = insets.Right * width;
                style.paddingTop = insets.Top * height;
                style.paddingBottom = insets.Bottom * height;
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent change) => Resized?.Invoke();
    }
}
