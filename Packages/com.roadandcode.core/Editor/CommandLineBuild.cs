using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RoadAndCode.Core.Editor
{
    /// <summary>
    /// Builds the enabled scenes from a terminal, with the project's own player settings:
    /// <code>Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod RoadAndCode.Core.Editor.CommandLineBuild.Build</code>
    /// Optional: <c>-outputPath &lt;dir&gt;</c> (defaults to Builds/&lt;target&gt;).
    /// </summary>
    public static class CommandLineBuild
    {
        private const string OutputArgument = "-outputPath";

        public static void Build()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string outputDir = Argument(OutputArgument) ?? Path.Combine("Builds", target.ToString());
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("No enabled scenes in Build Settings.");
                EditorApplication.Exit(1);
                return;
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                locationPathName = LocationFor(target, outputDir),
                options = BuildOptions.None,
            });

            var summary = report.summary;
            Debug.Log($"{summary.result}: {target} -> {summary.outputPath} " +
                      $"({summary.totalSize / (1024f * 1024f):F1} MB in {summary.totalTime.TotalSeconds:F0}s)");
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        // WebGL builds to a folder; the others need a file name.
        private static string LocationFor(BuildTarget target, string directory)
        {
            string name = PlayerSettings.productName.Replace(" ", string.Empty);
            switch (target)
            {
                case BuildTarget.StandaloneWindows64: return Path.Combine(directory, name + ".exe");
                case BuildTarget.Android: return Path.Combine(directory, name + ".apk");
                default: return directory;
            }
        }

        private static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
