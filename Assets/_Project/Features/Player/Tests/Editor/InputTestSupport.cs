using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Player.Tests
{
    internal static class InputTestSupport
    {
        /// <summary>A private copy of the game's real input actions, so tests exercise the real bindings.</summary>
        public static InputActionAsset LoadActions()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(InputActionAsset), new[] { "Assets/_Project" });
            Assert.That(guids, Has.Length.EqualTo(1), "Expected exactly one input actions asset in the project.");

            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return Object.Instantiate(asset);
        }

        public static List<PlayerAction> Drain(IPlayerActionSource source)
        {
            var actions = new List<PlayerAction>();
            while (source.TryDequeue(out var action)) actions.Add(action);
            return actions;
        }
    }

    internal sealed class FixedScreen : IScreenMetrics
    {
        public FixedScreen(float shortSide)
        {
            ShortSide = shortSide;
        }

        public float ShortSide { get; }
    }

    internal sealed class FakeInputProfile : IInputProfile
    {
        private readonly string[] _schemes;

        public FakeInputProfile(params string[] schemes)
        {
            _schemes = schemes;
        }

        public IReadOnlyList<string> ControlSchemes => _schemes;

        public bool Uses(string controlScheme) => _schemes.Contains(controlScheme);
    }
}
