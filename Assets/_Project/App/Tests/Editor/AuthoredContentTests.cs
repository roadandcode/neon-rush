using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Shared.Track;
using RoadAndCode.NeonRush.Track.Data;
using RoadAndCode.NeonRush.Track.Presentation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace RoadAndCode.NeonRush.App.Tests
{
    /// <summary>
    /// Checks the authored assets against each other. Player tuning and track patterns live in
    /// different features and are edited separately, so this is where a change to jump height
    /// that makes a pattern impossible gets caught.
    /// </summary>
    public sealed class AuthoredContentTests
    {
        private static T[] LoadAll<T>() where T : Object
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .ToArray();
        }

        private static T LoadOnly<T>() where T : Object
        {
            var assets = LoadAll<T>();
            Assert.That(assets, Has.Length.EqualTo(1), $"Expected exactly one {typeof(T).Name} asset.");
            return assets[0];
        }

        private static IEnumerable<string> PatternNames() => LoadAll<TrackPatternAsset>().Select(pattern => pattern.name);

        private static bool CanPass(TrackEntityDefinition cell, PlayerTuning tuning)
        {
            if (cell == null || cell is PickupDefinition) return true;

            bool jumpOver = cell.Top < tuning.JumpHeight;
            bool slideUnder = cell.Bottom > tuning.SlidingHeight;
            return jumpOver || slideUnder;
        }

        [Test]
        public void ThereArePatterns_AndAtLeastOneIsAvailableFromTheStart()
        {
            var patterns = LoadAll<TrackPatternAsset>();

            Assert.That(patterns, Is.Not.Empty);
            Assert.That(patterns.Any(pattern => pattern.MinTier == 0), Is.True);
        }

        [Test]
        public void TheTrackSettings_ListEveryPatternExactlyOnce()
        {
            var settings = LoadOnly<TrackSettingsAsset>();
            var listed = settings.Patterns.Cast<TrackPatternAsset>().Select(pattern => pattern.name).ToArray();
            var authored = LoadAll<TrackPatternAsset>().Select(pattern => pattern.name).ToArray();

            Assert.That(listed, Is.Unique);
            Assert.That(listed, Is.EquivalentTo(authored), "A pattern asset that is not in the settings never appears in the game.");
        }

        [TestCaseSource(nameof(PatternNames))]
        public void EveryRow_FitsTheLanes_AndCanBeSurvived(string patternName)
        {
            var pattern = LoadAll<TrackPatternAsset>().Single(candidate => candidate.name == patternName);
            var tuning = LoadOnly<PlayerTuningAsset>().Tuning;
            ILaneLayout lanes = LoadOnly<LaneLayoutAsset>().CreateGrid();

            Assert.That(pattern.RowCount, Is.GreaterThan(0));

            for (int row = 0; row < pattern.RowCount; row++)
            {
                Assert.That(pattern.CellAt(row, lanes.LaneCount), Is.Null, $"Row {row} has more cells than there are lanes.");

                bool survivable = Enumerable.Range(0, lanes.LaneCount)
                    .Any(lane => CanPass((TrackEntityDefinition)pattern.CellAt(row, lane), tuning));
                Assert.That(survivable, Is.True, $"Row {row} cannot be dodged, jumped or slid under.");
            }
        }

        [Test]
        public void EveryTrackEntity_FitsInsideALane()
        {
            ILaneLayout lanes = LoadOnly<LaneLayoutAsset>().CreateGrid();
            var definitions = LoadAll<TrackEntityDefinition>();

            Assert.That(definitions, Is.Not.Empty);
            foreach (var definition in definitions)
            {
                Assert.That(definition.Size.x, Is.LessThanOrEqualTo(lanes.LaneWidth), $"{definition.name} is wider than a lane.");
            }
        }

        // A view that is not in an Addressables group loads fine in the editor and fails only in a
        // build, which is the worst place to find out.
        [Test]
        public void EveryTrackEntitysView_IsAnAddressablePrefab_WithAViewOnItsRoot()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            Assert.That(settings, Is.Not.Null, "The project has no Addressables settings.");

            foreach (var definition in LoadAll<TrackEntityDefinition>())
            {
                Assert.That(definition.View.RuntimeKeyIsValid(), Is.True, $"{definition.name} has no view.");

                var prefab = definition.View.editorAsset as GameObject;
                Assert.That(prefab, Is.Not.Null, $"{definition.name}: the view reference does not point at a prefab.");
                Assert.That(prefab.GetComponent<TrackEntityView>(), Is.Not.Null, $"{definition.name}: its prefab has no TrackEntityView on the root.");
                Assert.That(settings.FindAssetEntry(definition.View.AssetGUID), Is.Not.Null, $"{definition.name}: its prefab is not in an Addressables group.");
            }
        }

        [Test]
        public void HazardsMeantToBeJumpedOrSlidUnder_LeaveRealClearance()
        {
            var tuning = LoadOnly<PlayerTuningAsset>().Tuning;

            foreach (var hazard in LoadAll<HazardDefinition>())
            {
                // Anything the runner can't clear either way is a wall, which is a valid hazard too.
                if (hazard.Top < tuning.JumpHeight)
                {
                    Assert.That(tuning.JumpHeight - hazard.Top, Is.GreaterThan(0.25f), $"{hazard.name} is too tall to jump comfortably.");
                }

                if (hazard.Bottom > tuning.SlidingHeight)
                {
                    Assert.That(hazard.Bottom - tuning.SlidingHeight, Is.GreaterThan(0.15f), $"{hazard.name} hangs too low to slide under.");
                    Assert.That(hazard.Bottom, Is.LessThan(tuning.StandingHeight), $"{hazard.name} would not touch a standing runner.");
                }
            }
        }
    }
}
