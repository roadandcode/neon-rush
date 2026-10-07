using NUnit.Framework;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Input;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Player.Tests
{
    /// <summary>
    /// Drives the real action asset with virtual devices: a key or a finger goes in at the bottom
    /// layer and a game action has to come out at the top.
    /// </summary>
    public sealed class InputSourceTests : InputTestFixture
    {
        private const float ScreenShortSide = 1000f;

        private InputActionAsset _actions;
        private PlayerTuning _tuning;
        private FakePointerClaims _claims;

        public override void Setup()
        {
            base.Setup();
            _actions = InputTestSupport.LoadActions();
            _tuning = new PlayerTuning();
            _claims = new FakePointerClaims();
        }

        public override void TearDown()
        {
            _actions.Disable();
            Object.DestroyImmediate(_actions);
            base.TearDown();
        }

        private float SwipeDistance => ScreenShortSide * _tuning.SwipeThreshold;

        private SwipeActionSource Swipes()
        {
            var source = new SwipeActionSource(_actions, new FixedScreen(ScreenShortSide), _claims, _tuning);
            source.SetEnabled(true);
            return source;
        }

        private IPlayerActionSource SourceFor(IInputProfile profile)
        {
            return PlayerInputFactory.Create(profile, _actions, new FixedScreen(ScreenShortSide), _claims, _tuning);
        }

        // ------------------------------------------------------------ buttons

        [Test]
        public void KeyboardKeys_BecomeRunnerActions()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            using var source = new ButtonActionSource(_actions);
            source.SetEnabled(true);

            PressAndRelease(keyboard.aKey);
            PressAndRelease(keyboard.rightArrowKey);
            PressAndRelease(keyboard.spaceKey);
            PressAndRelease(keyboard.sKey);

            Assert.That(InputTestSupport.Drain(source), Is.EqualTo(new[]
            {
                PlayerAction.MoveLeft, PlayerAction.MoveRight, PlayerAction.Jump, PlayerAction.Slide,
            }));
        }

        [Test]
        public void GamepadControls_BecomeTheSameActions()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            using var source = new ButtonActionSource(_actions);
            source.SetEnabled(true);

            PressAndRelease(gamepad.dpad.left);
            PressAndRelease(gamepad.buttonSouth);
            PressAndRelease(gamepad.buttonEast);

            Assert.That(InputTestSupport.Drain(source), Is.EqualTo(new[]
            {
                PlayerAction.MoveLeft, PlayerAction.Jump, PlayerAction.Slide,
            }));
        }

        [Test]
        public void ADisabledSource_HearsNothing_AndForgetsWhatItHad()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            using var source = new ButtonActionSource(_actions);
            source.SetEnabled(true);
            PressAndRelease(keyboard.aKey);

            source.SetEnabled(false);
            PressAndRelease(keyboard.dKey);

            Assert.That(InputTestSupport.Drain(source), Is.Empty);
        }

        [Test]
        public void AKeyHeldWhenTheSourceIsEnabled_DoesNotFire()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            using var source = new ButtonActionSource(_actions);

            // Space starts a run from the menu and is also jump. It must not do both.
            Press(keyboard.spaceKey);
            source.SetEnabled(true);
            InputSystem.Update();

            Assert.That(InputTestSupport.Drain(source), Is.Empty);
        }

        // ------------------------------------------------------------ gestures

        [Test]
        public void TouchSwipes_BecomeRunnerActions()
        {
            InputSystem.AddDevice<Touchscreen>();
            using var source = Swipes();
            var start = new Vector2(500f, 500f);
            float far = SwipeDistance * 1.5f;

            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(far, 0f));
            EndTouch(1, start + new Vector2(far, 0f));

            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(-far, 0f));
            EndTouch(1, start + new Vector2(-far, 0f));

            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(0f, far));
            EndTouch(1, start + new Vector2(0f, far));

            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(0f, -far));
            EndTouch(1, start + new Vector2(0f, -far));

            Assert.That(InputTestSupport.Drain(source), Is.EqualTo(new[]
            {
                PlayerAction.MoveRight, PlayerAction.MoveLeft, PlayerAction.Jump, PlayerAction.Slide,
            }));
        }

        [Test]
        public void ATap_IsNotASwipe()
        {
            InputSystem.AddDevice<Touchscreen>();
            using var source = Swipes();
            var start = new Vector2(500f, 500f);

            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(SwipeDistance * 0.4f, 0f));
            EndTouch(1, start + new Vector2(SwipeDistance * 0.4f, 0f));

            Assert.That(InputTestSupport.Drain(source), Is.Empty);
        }

        [Test]
        public void OneContinuousDrag_CanGiveTwoActions()
        {
            InputSystem.AddDevice<Touchscreen>();
            using var source = Swipes();
            var start = new Vector2(500f, 500f);
            float far = SwipeDistance * 1.5f;

            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(far, 0f));
            MoveTouch(1, start + new Vector2(far, far));
            EndTouch(1, start + new Vector2(far, far));

            Assert.That(InputTestSupport.Drain(source), Is.EqualTo(new[] { PlayerAction.MoveRight, PlayerAction.Jump }));
        }

        [Test]
        public void AMouseDrag_IsTheSameGestureAsATouch()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            using var source = Swipes();
            var start = new Vector2(300f, 300f);

            Move(mouse.position, start);
            Press(mouse.leftButton);
            Move(mouse.position, start + new Vector2(-SwipeDistance * 1.5f, 0f));
            Release(mouse.leftButton);

            Assert.That(InputTestSupport.Drain(source), Is.EqualTo(new[] { PlayerAction.MoveLeft }));
        }

        [Test]
        public void MovingTheMouseWithoutPressing_DoesNothing()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            using var source = Swipes();

            Move(mouse.position, new Vector2(100f, 100f));
            Move(mouse.position, new Vector2(900f, 100f));

            Assert.That(InputTestSupport.Drain(source), Is.Empty);
        }

        [Test]
        public void ADragThatStartsOnAnOnScreenControl_IsNotASwipe()
        {
            InputSystem.AddDevice<Touchscreen>();
            using var source = Swipes();
            _claims.Claimed = new Rect(800f, 800f, 200f, 200f);
            var onTheButton = new Vector2(900f, 900f);
            float far = SwipeDistance * 1.5f;

            // Starts on the button and leaves it: still the button's press.
            BeginTouch(1, onTheButton);
            MoveTouch(1, onTheButton + new Vector2(-far * 4f, 0f));
            EndTouch(1, onTheButton + new Vector2(-far * 4f, 0f));

            Assert.That(InputTestSupport.Drain(source), Is.Empty);
        }

        [Test]
        public void ADragThatEndsOnAnOnScreenControl_IsStillASwipe()
        {
            InputSystem.AddDevice<Touchscreen>();
            using var source = Swipes();
            _claims.Claimed = new Rect(800f, 400f, 200f, 200f);
            var start = new Vector2(500f, 500f);

            BeginTouch(1, start);
            MoveTouch(1, new Vector2(900f, 500f));
            EndTouch(1, new Vector2(900f, 500f));

            Assert.That(InputTestSupport.Drain(source), Is.EqualTo(new[] { PlayerAction.MoveRight }));
        }

        // ------------------------------------------------------------ composition per platform

        [Test]
        public void AButtonOnlyProfile_GetsNoGestureSource()
        {
            var profile = new FakeInputProfile(InputNames.Schemes.Keyboard, InputNames.Schemes.Gamepad);

            var source = SourceFor(profile);

            Assert.That(source, Is.InstanceOf<ButtonActionSource>());
        }

        [Test]
        public void ATouchOnlyProfile_GetsOnlyTheGestureSource()
        {
            var profile = new FakeInputProfile(InputNames.Schemes.Pointer);

            var source = SourceFor(profile);

            Assert.That(source, Is.InstanceOf<SwipeActionSource>());
        }

        [Test]
        public void AProfileWithBoth_HearsBothThroughOneSource()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Touchscreen>();
            var profile = new FakeInputProfile(InputNames.Schemes.Keyboard, InputNames.Schemes.Pointer);
            var source = SourceFor(profile);
            source.SetEnabled(true);
            var start = new Vector2(500f, 500f);

            PressAndRelease(keyboard.aKey);
            BeginTouch(1, start);
            MoveTouch(1, start + new Vector2(0f, SwipeDistance * 1.5f));
            EndTouch(1, start + new Vector2(0f, SwipeDistance * 1.5f));

            Assert.That(source, Is.InstanceOf<CompositeActionSource>());
            Assert.That(InputTestSupport.Drain(source), Is.EquivalentTo(new[] { PlayerAction.MoveLeft, PlayerAction.Jump }));
        }

        [Test]
        public void AProfileWithNothingUsable_IsRejected()
        {
            var profile = new FakeInputProfile("Steering Wheel");

            Assert.Throws<System.InvalidOperationException>(() => SourceFor(profile));
        }
    }
}
