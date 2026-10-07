using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Gestures;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Player.Input
{
    /// <summary>
    /// Gestures: a finger, mouse or pen dragged across the screen. Swipe sideways to change lane,
    /// up to jump, down to slide. This class only connects the pointer to the recogniser; the
    /// decision about what counts as a swipe is in <see cref="SwipeRecognizer"/>.
    /// </summary>
    internal sealed class SwipeActionSource : IPlayerActionSource, IDisposable
    {
        private readonly InputActionMap _map;
        private readonly InputAction _press;
        private readonly InputAction _position;
        private readonly IScreenMetrics _screen;
        private readonly IPointerClaims _claims;
        private readonly PlayerTuning _tuning;
        private readonly SwipeRecognizer _recognizer = new SwipeRecognizer();
        private readonly ActionQueue _queue = new ActionQueue();

        // The device whose press started the gesture in progress.
        private InputDevice _gestureDevice;

        public SwipeActionSource(InputActionAsset actions, IScreenMetrics screen, IPointerClaims claims, PlayerTuning tuning)
        {
            Guard.NotNull(actions, nameof(actions));
            _screen = Guard.NotNull(screen, nameof(screen));
            _claims = Guard.NotNull(claims, nameof(claims));
            _tuning = Guard.NotNull(tuning, nameof(tuning));

            _map = actions.FindActionMap(InputNames.Pointer.Map, throwIfNotFound: true);
            _press = _map.FindAction(InputNames.Pointer.Press, throwIfNotFound: true);
            _position = _map.FindAction(InputNames.Pointer.Position, throwIfNotFound: true);

            _press.started += OnPressed;
            _press.canceled += OnReleased;
            _position.performed += OnMoved;
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                _map.Enable();
                return;
            }

            _map.Disable();
            EndGesture();
            _queue.Clear();
        }

        public bool TryDequeue(out PlayerAction action) => _queue.TryDequeue(out action);

        public void Dispose()
        {
            _press.started -= OnPressed;
            _press.canceled -= OnReleased;
            _position.performed -= OnMoved;
            _map.Disable();
        }

        private void OnPressed(InputAction.CallbackContext context)
        {
            // Read the device, not the position action: for a new touch the press arrives first,
            // and the action would still hold wherever the previous contact ended.
            var pointer = context.control.device as Pointer;
            Vector2 position = pointer != null ? pointer.position.ReadValue() : _position.ReadValue<Vector2>();

            // A press that lands on an on-screen control belongs to that control, however far it then drags.
            if (_claims.IsClaimed(position)) return;

            _gestureDevice = context.control.device;
            _recognizer.Begin(position);
        }

        private void OnReleased(InputAction.CallbackContext context) => EndGesture();

        private void OnMoved(InputAction.CallbackContext context)
        {
            // A gesture belongs to the device that started it. On a machine with both a touchscreen
            // and a mouse, the position action reports every pointer, and the mouse resting somewhere
            // else on the screen must not steer a finger's swipe.
            if (!_recognizer.IsTracking || context.control.device != _gestureDevice) return;

            // The threshold is a share of the screen, so a swipe is the same physical gesture on a phone and a tablet.
            float threshold = _screen.ShortSide * _tuning.SwipeThreshold;
            switch (_recognizer.Move(context.ReadValue<Vector2>(), threshold))
            {
                case SwipeDirection.Left: _queue.Enqueue(PlayerAction.MoveLeft); break;
                case SwipeDirection.Right: _queue.Enqueue(PlayerAction.MoveRight); break;
                case SwipeDirection.Up: _queue.Enqueue(PlayerAction.Jump); break;
                case SwipeDirection.Down: _queue.Enqueue(PlayerAction.Slide); break;
            }
        }

        private void EndGesture()
        {
            _recognizer.End();
            _gestureDevice = null;
        }
    }
}
