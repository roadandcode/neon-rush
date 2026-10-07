using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>
    /// The one flow shortcut that works whatever is on screen: pause, which toggles. Starting a
    /// run and choosing between options go through the screens' own navigation.
    /// </summary>
    internal sealed class FlowInput : IStartable, IDisposable
    {
        private readonly IGameFlow _flow;
        private readonly InputActionMap _map;
        private readonly InputAction _pause;

        public FlowInput(IGameFlow flow, InputActionAsset asset)
        {
            _flow = Guard.NotNull(flow, nameof(flow));
            Guard.NotNull(asset, nameof(asset));

            _map = asset.FindActionMap(InputNames.Flow.Map, throwIfNotFound: true);
            _pause = _map.FindAction(InputNames.Flow.Pause, throwIfNotFound: true);
        }

        public void Start()
        {
            _pause.performed += OnPause;
            _map.Enable();
        }

        public void Dispose()
        {
            _pause.performed -= OnPause;
            _map.Disable();
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (!_flow.Pause()) _flow.Resume();
        }
    }
}
