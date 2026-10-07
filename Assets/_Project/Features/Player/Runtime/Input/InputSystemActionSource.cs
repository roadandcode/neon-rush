using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Player.Input
{
    /// <summary>
    /// Turns Input System button presses into queued <see cref="PlayerAction"/>s. Presses arrive
    /// as callbacks between ticks, so they wait in a small ring buffer until the simulation reads them.
    /// </summary>
    internal sealed class InputSystemActionSource : IPlayerActionSource, IDisposable
    {
        private const int Capacity = 8;

        private readonly InputActionMap _map;
        private readonly InputAction _moveLeft;
        private readonly InputAction _moveRight;
        private readonly InputAction _jump;
        private readonly InputAction _slide;

        private readonly PlayerAction[] _queue = new PlayerAction[Capacity];
        private int _head;
        private int _count;

        public InputSystemActionSource(InputActionAsset asset)
        {
            Guard.NotNull(asset, nameof(asset));

            _map = asset.FindActionMap(InputNames.Run.Map, throwIfNotFound: true);
            _moveLeft = _map.FindAction(InputNames.Run.MoveLeft, throwIfNotFound: true);
            _moveRight = _map.FindAction(InputNames.Run.MoveRight, throwIfNotFound: true);
            _jump = _map.FindAction(InputNames.Run.Jump, throwIfNotFound: true);
            _slide = _map.FindAction(InputNames.Run.Slide, throwIfNotFound: true);

            _moveLeft.performed += OnMoveLeft;
            _moveRight.performed += OnMoveRight;
            _jump.performed += OnJump;
            _slide.performed += OnSlide;
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                _map.Enable();
                return;
            }

            _map.Disable();
            _head = 0;
            _count = 0;
        }

        public bool TryDequeue(out PlayerAction action)
        {
            if (_count == 0)
            {
                action = default;
                return false;
            }

            action = _queue[_head];
            _head = (_head + 1) % Capacity;
            _count--;
            return true;
        }

        public void Dispose()
        {
            _moveLeft.performed -= OnMoveLeft;
            _moveRight.performed -= OnMoveRight;
            _jump.performed -= OnJump;
            _slide.performed -= OnSlide;
            _map.Disable();
        }

        private void OnMoveLeft(InputAction.CallbackContext context) => Enqueue(PlayerAction.MoveLeft);

        private void OnMoveRight(InputAction.CallbackContext context) => Enqueue(PlayerAction.MoveRight);

        private void OnJump(InputAction.CallbackContext context) => Enqueue(PlayerAction.Jump);

        private void OnSlide(InputAction.CallbackContext context) => Enqueue(PlayerAction.Slide);

        private void Enqueue(PlayerAction action)
        {
            // More presses than this between two ticks is a stuck key, not a player. Drop the excess.
            if (_count == Capacity) return;

            _queue[(_head + _count) % Capacity] = action;
            _count++;
        }
    }
}
