using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// A view that lives inside the shared UI document. Unity builds the document's visual tree
    /// when the document is enabled and throws it away when it is disabled, so views look their
    /// elements up on <see cref="Bind"/> and let go of them on <see cref="Unbind"/>.
    /// </summary>
    internal interface IDocumentView
    {
        void Bind(VisualElement documentRoot);

        void Unbind();
    }
}
