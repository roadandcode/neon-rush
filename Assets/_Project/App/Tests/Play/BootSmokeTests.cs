using System.Collections;
using NUnit.Framework;
using RoadAndCode.NeonRush.App.Composition;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Run;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace RoadAndCode.NeonRush.App.PlayTests
{
    /// <summary>
    /// Boots the real scenes with the real containers. If a registration is missing, a scene
    /// reference is unassigned or a constructor can't be satisfied, this is the test that says so.
    /// Any logged error fails it.
    /// </summary>
    public sealed class BootSmokeTests
    {
        private const float BootTimeoutSeconds = 20f;
        private const float RunTimeoutSeconds = 20f;

        [UnityTest]
        public IEnumerator Boot_LoadsGameplay_AndARunAdvancesUntilItIsPausedOrFailed()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Bootstrap, LoadSceneMode.Single);

            var app = Object.FindAnyObjectByType<AppLifetimeScope>();
            Assert.That(app, Is.Not.Null, "Bootstrap scene has no AppLifetimeScope.");
            var flow = app.Container.Resolve<IGameFlow>();

            float deadline = Time.realtimeSinceStartup + BootTimeoutSeconds;
            while (flow.Phase != GamePhase.Menu && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Menu), "Boot did not reach the menu.");

            var gameplay = Object.FindAnyObjectByType<GameplayLifetimeScope>();
            Assert.That(gameplay, Is.Not.Null, "Gameplay scene was not loaded.");
            var progress = gameplay.Container.Resolve<IRunProgress>();

            // Nothing moves on the menu.
            yield return null;
            Assert.That(progress.Distance, Is.Zero);

            Assert.That(flow.StartRun(), Is.True);
            deadline = Time.realtimeSinceStartup + RunTimeoutSeconds;
            while (progress.Distance < 5f && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(progress.Distance, Is.GreaterThanOrEqualTo(5f), "The run did not advance.");

            // Paused: the clock stops.
            Assert.That(flow.Pause(), Is.True);
            float pausedAt = progress.Distance;
            yield return null;
            yield return null;
            Assert.That(progress.Distance, Is.EqualTo(pausedAt));

            Assert.That(flow.Resume(), Is.True);
            yield return null;

            // With nobody steering the run may already have ended in a crash; either way it must end cleanly.
            if (flow.Phase == GamePhase.Run) Assert.That(flow.FailRun(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.GameOver));

            Assert.That(flow.StartRun(), Is.True, "A new run should start from game over.");
            yield return null;
            Assert.That(progress.Distance, Is.LessThan(pausedAt), "The new run should have started from zero.");
        }
    }
}
