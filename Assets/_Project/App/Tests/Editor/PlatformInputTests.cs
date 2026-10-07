using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.App.Input;
using RoadAndCode.NeonRush.Player.Input;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.App.Tests
{
    /// <summary>
    /// The platform layer of input: a profile decides which device families the game listens to,
    /// and everything above it gets that for free.
    /// </summary>
    public sealed class PlatformInputTests : InputTestFixture
    {
        private sealed class Profile : IInputProfile
        {
            private readonly string[] _schemes;

            public Profile(params string[] schemes)
            {
                _schemes = schemes;
            }

            public IReadOnlyList<string> ControlSchemes => _schemes;

            public bool Uses(string controlScheme) => _schemes.Contains(controlScheme);
        }

        private static InputActionAsset SourceAsset()
        {
            string guid = AssetDatabase.FindAssets("t:" + nameof(InputActionAsset), new[] { "Assets/_Project" }).Single();
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private static List<PlayerAction> Drain(IPlayerActionSource source)
        {
            var actions = new List<PlayerAction>();
            while (source.TryDequeue(out var action)) actions.Add(action);
            return actions;
        }

        [Test]
        public void TheGameGetsItsOwnCopy_SoTheAssetIsNeverModified()
        {
            var asset = SourceAsset();

            using var input = new PlatformInput(asset, new Profile(InputNames.Schemes.Gamepad));

            Assert.That(input.Actions, Is.Not.SameAs(asset));
            Assert.That(asset.bindingMask, Is.Null);
            Assert.That(input.Actions.bindingMask, Is.Not.Null);
        }

        [Test]
        public void OnAPlatformWithoutAKeyboardScheme_KeysDoNothing_ButTheGamepadWorks()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            using var input = new PlatformInput(SourceAsset(), new Profile(InputNames.Schemes.Gamepad, InputNames.Schemes.Pointer));
            using var buttons = new ButtonActionSource(input.Actions);
            buttons.SetEnabled(true);

            PressAndRelease(keyboard.aKey);
            Assert.That(Drain(buttons), Is.Empty);

            PressAndRelease(gamepad.dpad.right);
            Assert.That(Drain(buttons), Is.EqualTo(new[] { PlayerAction.MoveRight }));
        }

        [Test]
        public void OnAPlatformWithAKeyboardScheme_KeysWork()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            using var input = new PlatformInput(SourceAsset(), new Profile(InputNames.Schemes.Keyboard, InputNames.Schemes.Gamepad));
            using var buttons = new ButtonActionSource(input.Actions);
            buttons.SetEnabled(true);

            PressAndRelease(keyboard.aKey);

            Assert.That(Drain(buttons), Is.EqualTo(new[] { PlayerAction.MoveLeft }));
        }

        [Test]
        public void TheAuthoredProfiles_CoverEveryPlatform_AndOnlyNameRealSchemes()
        {
            var realSchemes = SourceAsset().controlSchemes.Select(scheme => scheme.name).ToArray();
            var profiles = AssetDatabase.FindAssets("t:" + nameof(InputProfileAsset))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<InputProfileAsset>)
                .ToArray();

            Assert.That(profiles, Has.Length.EqualTo(System.Enum.GetValues(typeof(PlatformKind)).Length), "One input profile per platform family.");
            foreach (var profile in profiles)
            {
                Assert.That(profile.ControlSchemes, Is.Not.Empty, profile.name);
                Assert.That(profile.ControlSchemes, Is.SubsetOf(realSchemes), $"{profile.name} names a control scheme the actions asset doesn't have.");
            }
        }

        [Test]
        public void EveryBinding_BelongsToExactlyOneControlScheme()
        {
            var asset = SourceAsset();
            var realSchemes = asset.controlSchemes.Select(scheme => scheme.bindingGroup).ToArray();

            foreach (var binding in asset.actionMaps.SelectMany(map => map.bindings))
            {
                Assert.That(realSchemes, Does.Contain(binding.groups), $"Binding '{binding.path}' on '{binding.action}' is in no control scheme, so no profile could switch it off.");
            }
        }
    }
}
