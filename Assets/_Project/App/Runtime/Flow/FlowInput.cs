using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>
    /// Keyboard and gamepad shortcuts for the flow itself: confirm starts or restarts a run,
    /// pause toggles. On-screen buttons call the same <see cref="IGameFlow"/> methods.
    /// </summary>
    internal sealed class FlowInput : IStartable, IDisposable
    {
        private readonly IGameFlow _flow;
        private readonly InputActionMap _map;
        private readonly InputAction _confirm;
        private readonly InputAction _pause;

        public FlowInput(IGameFlow flow, InputActionAsset asset)
        {
            _flow = Guard.NotNull(flow, nameof(flow));
            Guard.NotNull(asset, nameof(asset));

            _map = asset.FindActionMap(InputNames.Flow.Map, throwIfNotFound: true);
            _confirm = _map.FindAction(InputNames.Flow.Confirm, throwIfNotFound: true);
            _pause = _map.FindAction(InputNames.Flow.Pause, throwIfNotFound: true);
        }

        public void Start()
        {
            _confirm.performed += OnConfirm;
            _pause.performed += OnPause;
            _map.Enable();
        }

        public void Dispose()
        {
            _confirm.performed -= OnConfirm;
            _pause.performed -= OnPause;
            _map.Disable();
        }

        // StartRun refuses unless the game is on the menu or the game-over screen, so no phase check is needed.
        private void OnConfirm(InputAction.CallbackContext context) => _flow.StartRun();

        private void OnPause(InputAction.CallbackContext context)
        {
            if (!_flow.Pause()) _flow.Resume();
        }
    }
}
