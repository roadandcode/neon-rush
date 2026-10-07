using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadAndCode.NeonRush.Screens.Logic;
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
            yield return new ImpactFlashView();
            yield return new ButtonPressView();
            yield return new SoundToggleView();
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

        // The menu and the pause panel both offer it; a screen that loses its copy fails here.
        [Test]
        public void TheSoundToggle_IsOnTheMenuAndThePausePanel()
        {
            foreach (string screen in new[] { "menu", "pause" })
            {
                var toggles = _document.Q(screen).Query<Button>(className: "sound-toggle").ToList();

                Assert.That(toggles, Has.Count.EqualTo(1), $"'{screen}'");
            }
        }

        [Test]
        public void TheSoundToggle_ShowsItsStateOnEveryCopy()
        {
            var view = new SoundToggleView();
            view.Bind(_document);

            view.SetSoundOn(false);
            Assert.That(_document.Query<Button>(className: "sound-toggle--off").ToList(), Has.Count.EqualTo(2));

            view.SetSoundOn(true);
            Assert.That(_document.Query<Button>(className: "sound-toggle--off").ToList(), Is.Empty);
        }

        [Test]
        public void TheFlash_SwitchesOn_WhenAsked()
        {
            var view = new ImpactFlashView();
            view.Bind(_document);

            view.Flash();

            Assert.That(_document.Q("impact-flash").ClassListContains("flash--on"), Is.True);
        }

        // What keys and a gamepad step through, in order. Play, retry and resume are first, so submit alone does the obvious thing.
        [Test]
        public void EachScreensControls_AreItsButtons_InReadingOrder()
        {
            var menu = new MenuView();
            var pause = new PauseView();
            var gameOver = new GameOverView();
            menu.Bind(_document);
            pause.Bind(_document);
            gameOver.Bind(_document);

            Assert.That(menu.ControlCount, Is.EqualTo(2));
            Assert.That(pause.ControlCount, Is.EqualTo(3));
            Assert.That(gameOver.ControlCount, Is.EqualTo(2));

            Assert.That(_document.Q("menu").Query<Button>().First().name, Is.EqualTo("menu-play"));
            Assert.That(_document.Q("pause").Query<Button>().First().name, Is.EqualTo("pause-resume"));
            Assert.That(_document.Q("game-over").Query<Button>().First().name, Is.EqualTo("game-over-retry"));
        }

        [Test]
        public void FocusingAControl_MarksThatButtonAndNoOther()
        {
            var pause = new PauseView();
            pause.Bind(_document);

            pause.SetFocus(1);
            var focused = _document.Query<Button>(className: "focused").ToList();
            Assert.That(focused, Has.Count.EqualTo(1));
            Assert.That(focused[0].name, Is.EqualTo("pause-quit"));

            pause.SetFocus(ControlFocus.None);
            Assert.That(_document.Query<Button>(className: "focused").ToList(), Is.Empty);
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
