using UnityEngine;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// The UI's only MonoBehaviour. It owns the views and connects them to the document's visual
    /// tree whenever Unity builds one. The views exist from construction, so the container can
    /// hand them to presenters before the tree is ready; nothing calls into them until start-up.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScreensDocument : MonoBehaviour
    {
        [Tooltip("The document holding every screen. All screens share one panel, so the UI is one draw pass.")]
        [SerializeField] private UIDocument _document;

        private IDocumentView[] _views;

        internal MenuView Menu { get; } = new MenuView();

        internal HudView Hud { get; } = new HudView();

        internal PauseView Pause { get; } = new PauseView();

        internal GameOverView GameOver { get; } = new GameOverView();

        internal ControlHintsView ControlHints { get; } = new ControlHintsView();

        internal SafeAreaView SafeArea { get; } = new SafeAreaView();

        internal PanelPointerClaims PointerClaims { get; } = new PanelPointerClaims();

        // UIDocument builds its tree in its own OnEnable, which Unity runs before this one.
        private void OnEnable()
        {
            VisualElement root = _document.rootVisualElement;
            if (root == null)
            {
                Debug.LogError($"{nameof(ScreensDocument)} on '{name}': the UI document has no visual tree. Check its source asset and panel settings.", this);
                return;
            }

            _views ??= new IDocumentView[] { Menu, Hud, Pause, GameOver, ControlHints, SafeArea, PointerClaims };
            foreach (IDocumentView view in _views) view.Bind(root);
        }

        private void OnDisable()
        {
            if (_views == null) return;
            foreach (IDocumentView view in _views) view.Unbind();
        }

        private void OnValidate()
        {
            if (_document == null) Debug.LogError($"{nameof(ScreensDocument)} on '{name}' has no UI document.", this);
        }
    }
}
