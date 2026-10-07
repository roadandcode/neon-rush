using System.Threading;
using RoadAndCode.NeonRush.App.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Composition
{
    /// <summary>
    /// Start-up, in order: enter the Boot phase, load the gameplay scene under the app scope,
    /// then open the menu. Anything that has to be ready before the menu shows is awaited here.
    /// </summary>
    internal sealed class BootSequence : IAsyncStartable
    {
        private readonly GameFlow _flow;
        private readonly LifetimeScope _appScope;

        public BootSequence(GameFlow flow, LifetimeScope appScope)
        {
            _flow = flow;
            _appScope = appScope;
        }

        public async Awaitable StartAsync(CancellationToken cancellation)
        {
            _flow.Start();

            // The gameplay scene's scope becomes a child of this one, which is how gameplay
            // systems receive the message bus, the save store and the flow.
            using (LifetimeScope.EnqueueParent(_appScope))
            {
                await SceneManager.LoadSceneAsync(SceneNames.Gameplay, LoadSceneMode.Additive);
            }

            cancellation.ThrowIfCancellationRequested();
            _flow.FinishBoot();
        }
    }
}
