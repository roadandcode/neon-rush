using RoadAndCode.NeonRush.Screens.Logic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// The flash is two style states and a transition, all in the style sheet: "on" is opaque
    /// with no transition, and taking it away fades out. This class only switches between them.
    /// </summary>
    internal sealed class ImpactFlashView : IImpactFlashView, IDocumentView
    {
        private const string ElementName = "impact-flash";
        private const string OnClass = "flash--on";

        // Long enough for the "on" state to be drawn at least once before the fade starts.
        private const long HoldMilliseconds = 60;

        private VisualElement _flash;
        private IVisualElementScheduledItem _fade;

        public void Bind(VisualElement documentRoot)
        {
            _flash = documentRoot.Require<VisualElement>(ElementName);
            _fade = _flash.schedule.Execute(Fade);
            _fade.Pause();
        }

        public void Unbind()
        {
            _fade?.Pause();
            _fade = null;
            _flash = null;
        }

        public void Flash()
        {
            _flash.AddToClassList(OnClass);
            _fade.ExecuteLater(HoldMilliseconds);
        }

        private void Fade() => _flash.RemoveFromClassList(OnClass);
    }
}
