using System.Collections;
using NUnit.Framework;
using RoadAndCode.NeonRush.App.Composition;
using RoadAndCode.NeonRush.Shared.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace RoadAndCode.NeonRush.App.PlayTests
{
    /// <summary>
    /// Boots the real scene with the real container. If a registration is missing or a
    /// constructor can't be satisfied, this is the test that says so. Any logged error fails it.
    /// </summary>
    public sealed class BootSmokeTests
    {
        private const string BootstrapScene = "Bootstrap";

        [UnityTest]
        public IEnumerator BootstrapScene_ReachesTheMenu_AndCanRunPauseAndFail()
        {
            yield return SceneManager.LoadSceneAsync(BootstrapScene, LoadSceneMode.Single);
            yield return null;

            var scope = Object.FindAnyObjectByType<AppLifetimeScope>();
            Assert.That(scope, Is.Not.Null, "Bootstrap scene has no AppLifetimeScope.");

            var flow = scope.Container.Resolve<IGameFlow>();
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Menu));

            Assert.That(flow.StartRun(), Is.True);
            yield return null;
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Run));

            Assert.That(flow.Pause(), Is.True);
            Assert.That(flow.Resume(), Is.True);
            yield return null;

            Assert.That(flow.FailRun(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(flow.ReturnToMenu(), Is.True);
        }
    }
}
