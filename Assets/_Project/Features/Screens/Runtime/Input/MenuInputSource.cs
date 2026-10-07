using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine.InputSystem;

namespace RoadAndCode.NeonRush.Screens.Input
{
    /// <summary>
    /// Menu navigation from the actions asset. The asset it is given is already narrowed to the
    /// platform's control schemes, so on a platform with neither keys nor a pad this simply
    /// never fires. It is only switched on while a screen with controls is up, because the same
    /// keys steer the runner during a run.
    /// </summary>
    internal sealed class MenuInputSource : IMenuInput, IDisposable
    {
        private const int Back = -1;
        private const int Forward = 1;

        private readonly InputActionMap _map;
        private readonly InputAction _previous;
        private readonly InputAction _next;
        private readonly InputAction _submit;

        public MenuInputSource(InputActionAsset actions)
        {
            Guard.NotNull(actions, nameof(actions));

            _map = actions.FindActionMap(InputNames.Menu.Map, throwIfNotFound: true);
            _previous = _map.FindAction(InputNames.Menu.Previous, throwIfNotFound: true);
            _next = _map.FindAction(InputNames.Menu.Next, throwIfNotFound: true);
            _submit = _map.FindAction(InputNames.Menu.Submit, throwIfNotFound: true);

            _previous.performed += OnPrevious;
            _next.performed += OnNext;
            _submit.performed += OnSubmit;
        }

        public event Action<int> Moved;

        public event Action Submitted;

        public void SetEnabled(bool enabled)
        {
            if (enabled) _map.Enable();
            else _map.Disable();
        }

        public void Dispose()
        {
            _previous.performed -= OnPrevious;
            _next.performed -= OnNext;
            _submit.performed -= OnSubmit;
            _map.Disable();
        }

        private void OnPrevious(InputAction.CallbackContext context) => Moved?.Invoke(Back);

        private void OnNext(InputAction.CallbackContext context) => Moved?.Invoke(Forward);

        private void OnSubmit(InputAction.CallbackContext context) => Submitted?.Invoke();
    }
}
