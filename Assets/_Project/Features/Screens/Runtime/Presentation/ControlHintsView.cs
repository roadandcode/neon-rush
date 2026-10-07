using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// Hints are written in the UXML once per control scheme and hidden by default. Tagging the
    /// document root with "uses-keyboard", "uses-pointer" and so on lets the style sheet reveal
    /// the matching ones, so a new scheme needs markup and a style rule but no code.
    /// </summary>
    internal sealed class ControlHintsView : IControlHintsView, IDocumentView
    {
        private const string ClassPrefix = "uses-";

        private VisualElement _root;

        public void Bind(VisualElement documentRoot) => _root = documentRoot;

        public void Unbind() => _root = null;

        public void ShowHintsFor(string controlScheme) => _root.AddToClassList(ClassFor(controlScheme));

        internal static string ClassFor(string controlScheme) => ClassPrefix + controlScheme.ToLowerInvariant();
    }
}
