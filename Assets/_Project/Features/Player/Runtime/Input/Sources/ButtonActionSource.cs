using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Player.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Player.Input
{
    /// <summary>
    /// Buttons: keyboard keys and gamepad controls. Which physical buttons those are is decided by
    /// the bindings in the actions asset and by the platform's control schemes, not here.
    /// </summary>
    internal sealed class ButtonActionSource : IPlayerActionSource, IDisposable
    {
        private readonly InputActionMap _map;
        private readonly InputAction _moveLeft;
        private readonly InputAction _moveRight;
        private readonly InputAction _jump;
        private readonly InputAction _slide;
        private readonly ActionQueue _queue = new ActionQueue();

        public ButtonActionSource(InputActionAsset actions)
        {
            Guard.NotNull(actions, nameof(actions));

            _map = actions.FindActionMap(InputNames.Run.Map, throwIfNotFound: true);
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
            _queue.Clear();
        }

        public bool TryDequeue(out PlayerAction action) => _queue.TryDequeue(out action);

        public void Dispose()
        {
            _moveLeft.performed -= OnMoveLeft;
            _moveRight.performed -= OnMoveRight;
            _jump.performed -= OnJump;
            _slide.performed -= OnSlide;
            _map.Disable();
        }

        private void OnMoveLeft(InputAction.CallbackContext context) => _queue.Enqueue(PlayerAction.MoveLeft);

        private void OnMoveRight(InputAction.CallbackContext context) => _queue.Enqueue(PlayerAction.MoveRight);

        private void OnJump(InputAction.CallbackContext context) => _queue.Enqueue(PlayerAction.Jump);

        private void OnSlide(InputAction.CallbackContext context) => _queue.Enqueue(PlayerAction.Slide);
    }
}
