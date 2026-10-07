using System.Collections;
using NUnit.Framework;
using RoadAndCode.NeonRush.App.Composition;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace RoadAndCode.NeonRush.App.PlayTests
{
    /// <summary>
    /// The real UI document and the real camera in the real scene: the right screens are up in
    /// each phase, the opening shot ends behind the runner, and the panel claims pointer presses
    /// where its controls are and nowhere else.
    /// </summary>
    public sealed class ScreenSmokeTests
    {
        private const float BootTimeoutSeconds = 20f;
        private const float IntroTimeoutSeconds = 10f;
        private const float ResultTimeoutSeconds = 5f;
        private const string HiddenClass = "hidden";
        private const string Menu = "menu";
        private const string Hud = "hud";
        private const string Pause = "pause";
        private const string GameOver = "game-over";
        private const string PauseButton = "hud-pause";

        private VisualElement _root;

        [UnityTest]
        public IEnumerator Screens_FollowThePhase_TheCameraComesRound_AndOnlyControlsClaimThePointer()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Bootstrap, LoadSceneMode.Single);

            var app = Object.FindAnyObjectByType<AppLifetimeScope>();
            var flow = app.Container.Resolve<IGameFlow>();
            float deadline = Time.realtimeSinceStartup + BootTimeoutSeconds;
            while (flow.Phase != GamePhase.Menu && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Menu), "Boot did not reach the menu.");

            var gameplay = Object.FindAnyObjectByType<GameplayLifetimeScope>();
            var document = Object.FindAnyObjectByType<UIDocument>();
            Assert.That(document, Is.Not.Null, "Gameplay scene has no UI document.");
            _root = document.rootVisualElement;
            var claims = gameplay.Container.Resolve<IPointerClaims>();
            var screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // The screens start a frame after the scene that holds them, and lay out a frame after that.
            yield return null;
            yield return null;
            AssertUp(Menu);

            // On the menu the camera is ahead of the runner, looking back at it.
            Camera camera = Camera.main;
            Assert.That(camera, Is.Not.Null, "No main camera once the gameplay scene has loaded.");
            Assert.That(camera.transform.position.z, Is.GreaterThan(0f), "The menu shot should be in front of the runner.");
            Assert.That(camera.transform.forward.z, Is.LessThan(0f), "The menu shot should look back at the runner.");

            // Play: nothing on screen while the camera travels, then the HUD once the run begins.
            Assert.That(flow.StartRun(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Intro));
            yield return null;
            AssertUp();

            deadline = Time.realtimeSinceStartup + IntroTimeoutSeconds;
            while (flow.Phase == GamePhase.Intro && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Run), "The intro never handed over to the run.");
            yield return null;
            yield return null;
            AssertUp(Hud);
            Assert.That(camera.transform.position.z, Is.LessThan(0f), "The run shot should be behind the runner.");
            Assert.That(camera.transform.forward.z, Is.GreaterThan(0f), "The run shot should look down the track.");

            Assert.That(claims.IsClaimed(ScreenPositionOf(PauseButton)), Is.True, "The pause button should claim a press on it.");
            Assert.That(claims.IsClaimed(screenCentre), Is.False, "Open track should be free for gestures.");

            Assert.That(flow.Pause(), Is.True);
            yield return null;
            AssertUp(Hud, Pause);
            Assert.That(claims.IsClaimed(screenCentre), Is.True, "The pause backdrop should claim everything under it.");

            Assert.That(flow.Resume(), Is.True);
            if (flow.Phase == GamePhase.Run) Assert.That(flow.FailRun(), Is.True);
            yield return null;

            // The result is held back while the crash plays out, then appears on its own.
            AssertUp();
            deadline = Time.realtimeSinceStartup + ResultTimeoutSeconds;
            while (IsHidden(GameOver) && Time.realtimeSinceStartup < deadline) yield return null;
            AssertUp(GameOver);

            Assert.That(flow.ReturnToMenu(), Is.True);
            yield return null;
            AssertUp(Menu);
        }

        private void AssertUp(params string[] expected)
        {
            foreach (string screen in new[] { Menu, Hud, Pause, GameOver })
            {
                bool shouldBeUp = System.Array.IndexOf(expected, screen) >= 0;
                Assert.That(!IsHidden(screen), Is.EqualTo(shouldBeUp), $"'{screen}' screen");
            }
        }

        private bool IsHidden(string screen) => _root.Q(screen).ClassListContains(HiddenClass);

        // Panel positions count down from the top in panel units; screen positions count up from the bottom in pixels.
        private Vector2 ScreenPositionOf(string elementName)
        {
            Vector2 centre = _root.Q(elementName).worldBound.center;
            Rect panel = _root.panel.visualTree.layout;
            return new Vector2(centre.x / panel.width * Screen.width, (1f - centre.y / panel.height) * Screen.height);
        }
    }
}
