using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// A press is claimed wherever the UI panel would deliver it to an element. Containers and
    /// labels in the UXML are marked picking-mode="Ignore", which leaves buttons and full-screen
    /// backdrops as the only things that answer, and open track as free for gestures.
    /// </summary>
    internal sealed class PanelPointerClaims : IPointerClaims, IDocumentView
    {
        private VisualElement _root;

        public void Bind(VisualElement documentRoot) => _root = documentRoot;

        public void Unbind() => _root = null;

        public bool IsClaimed(Vector2 screenPosition)
        {
            IPanel panel = _root?.panel;
            if (panel == null) return false;

            // Screen positions count up from the bottom of the screen and panels count down from
            // the top. Scale to panel units first, then flip against the panel's own height.
            Vector2 point = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            point.y = panel.visualTree.layout.height - point.y;

            return panel.Pick(point) != null;
        }
    }
}
