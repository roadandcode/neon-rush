using System.Collections;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.App.Composition;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Scoring;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace RoadAndCode.NeonRush.App.PlayTests
{
    /// <summary>
    /// The score display on the real, live panel. A unit test can show that the game's own code
    /// builds no strings, but not what the UI framework does once an element is attached to a
    /// panel, and that is where the first version of the display turned out to allocate on every
    /// change. This measures whole frames, so it covers both.
    /// </summary>
    public sealed class HudAllocationTests
    {
        private const float TimeoutSeconds = 20f;
        private const int MeasuredFrames = 240;

        // Measured on a live panel: what one label costs when its text changes. The display must
        // come in far under that, with room for the odd unrelated allocation by the engine.
        private const long BytesPerTextChange = 364;
        private const long AllowedBytesPerChange = BytesPerTextChange / 4;

        [UnityTest]
        public IEnumerator ACountingScore_OnTheLivePanel_AllocatesNoMoreThanAnIdleFrame()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Bootstrap, LoadSceneMode.Single);

            var app = Object.FindAnyObjectByType<AppLifetimeScope>();
            var flow = app.Container.Resolve<IGameFlow>();
            var publisher = app.Container.Resolve<IPublisher>();

            float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            while (flow.Phase != GamePhase.Menu && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(flow.StartRun(), Is.True);
            while (flow.Phase != GamePhase.Run && Time.realtimeSinceStartup < deadline) yield return null;

            // Paused: the HUD is still up behind the pause panel and the simulation is stopped, so
            // the only thing changing from here on is the score this test sends.
            Assert.That(flow.Pause(), Is.True);

            using ProfilerRecorder allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

            // Turn every reel through every digit once, so first-use costs are paid before measuring.
            // Four digits throughout: gaining a digit shows a cell, which is allowed to allocate.
            for (int digit = 1; digit <= 9; digit++)
            {
                publisher.Publish(new ScoreChanged(digit * 1111, multiplier: 1));
                yield return null;
            }

            for (int i = 0; i < 20; i++) yield return null;

            var idle = new long[MeasuredFrames];
            for (int i = 0; i < MeasuredFrames; i++)
            {
                yield return null;
                idle[i] = allocated.LastValue;
            }

            var counting = new long[MeasuredFrames];
            int score = 1000;
            for (int i = 0; i < MeasuredFrames; i++)
            {
                publisher.Publish(new ScoreChanged(score += 37, multiplier: 1));
                yield return null;
                counting[i] = allocated.LastValue;
            }

            // The test harness itself allocates a little every frame, which is why this compares
            // against idle frames instead of against zero.
            Assert.That(Median(counting), Is.EqualTo(Median(idle)), "A typical frame with a score change should allocate exactly what an idle frame does.");

            long extraPerChange = (Sum(counting) - Sum(idle)) / MeasuredFrames;
            Assert.That(extraPerChange, Is.LessThan(AllowedBytesPerChange), $"Score changes cost {extraPerChange} B each on average; a label text change costs {BytesPerTextChange} B.");
        }

        private static long Sum(long[] values)
        {
            long sum = 0;
            foreach (long value in values) sum += value;
            return sum;
        }

        private static long Median(long[] values)
        {
            var sorted = (long[])values.Clone();
            System.Array.Sort(sorted);
            return sorted[sorted.Length / 2];
        }
    }
}
