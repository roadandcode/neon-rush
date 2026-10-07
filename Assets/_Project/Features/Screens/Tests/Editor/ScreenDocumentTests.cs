using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadAndCode.NeonRush.Screens.Presentation;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    /// <summary>
    /// Checks the authored UXML against the code that drives it. The views find their elements by
    /// name, and the UXML is edited separately, so this is where a renamed button or a control
    /// scheme with no hints gets caught before anyone has to run the game.
    /// </summary>
    public sealed class ScreenDocumentTests
    {
        private const string DocumentPath = "Assets/_Project/Content/UI/Screens.uxml";
        private const string HiddenClass = "hidden";
        private const string ScreenClass = "screen";
        private const string BackdropClass = "backdrop";
        private const string HintClassPrefix = "hint--";

        private static readonly string[] ControlSchemes =
        {
            InputNames.Schemes.Keyboard, InputNames.Schemes.Gamepad, InputNames.Schemes.Pointer,
        };

        private VisualElement _document;

        [SetUp]
        public void SetUp()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(DocumentPath);
            Assert.That(asset, Is.Not.Null, $"No UI document at {DocumentPath}.");
            _document = asset.CloneTree();
        }

        private static IEnumerable<IDocumentView> Views()
        {
            yield return new MenuView();
            yield return new HudView();
            yield return new PauseView();
            yield return new GameOverView();
            yield return new ControlHintsView();
            yield return new SafeAreaView();
            yield return new PanelPointerClaims();
        }

        private List<VisualElement> Descendants() => _document.Query<VisualElement>().ToList();

        [Test]
        public void EveryView_FindsItsElements()
        {
            foreach (IDocumentView view in Views())
            {
                Assert.DoesNotThrow(() => view.Bind(_document), view.GetType().Name);
                Assert.DoesNotThrow(view.Unbind, view.GetType().Name);
            }
        }

        [Test]
        public void EveryScreen_StartsHidden()
        {
            var screens = _document.Query<VisualElement>(className: ScreenClass).ToList();

            Assert.That(screens, Has.Count.EqualTo(4));
            foreach (VisualElement screen in screens)
            {
                Assert.That(screen.ClassListContains(HiddenClass), Is.True, $"'{screen.name}' would flash up before the game decides what to show.");
            }
        }

        // A focused button answers Space and Enter, which are also jump and start.
        [Test]
        public void NoButton_CanTakeKeyboardFocus()
        {
            var buttons = _document.Query<Button>().ToList();

            Assert.That(buttons, Is.Not.Empty);
            foreach (Button button in buttons)
            {
                Assert.That(button.focusable, Is.False, $"'{button.name}' is focusable.");
            }
        }

        // Anything else that takes pointer input would swallow swipes meant for the runner.
        [Test]
        public void OnlyButtonsAndBackdrops_TakePointerInput()
        {
            foreach (IDocumentView view in Views()) view.Bind(_document);

            var offenders = Descendants()
                .Where(element => element != _document)
                .Where(element => element.pickingMode == PickingMode.Position)
                .Where(element => !(element is Button) && !element.ClassListContains(BackdropClass))
                .Select(element => $"{element.GetType().Name} '{element.name}' ({string.Join(" ", element.GetClasses())})")
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void EveryControlScheme_HasHints()
        {
            foreach (string scheme in ControlSchemes)
            {
                string hintClass = HintClassPrefix + scheme.ToLowerInvariant();

                Assert.That(_document.Query<VisualElement>(className: hintClass).ToList(), Is.Not.Empty, $"Nothing is marked '{hintClass}'.");
            }
        }

        [Test]
        public void SomethingIsMarkedAsSafeArea_InEveryScreen()
        {
            foreach (VisualElement screen in _document.Query<VisualElement>(className: ScreenClass).ToList())
            {
                Assert.That(screen.Query<VisualElement>(className: "safe-area").ToList(), Has.Count.EqualTo(1), $"'{screen.name}'");
            }
        }
    }
}
