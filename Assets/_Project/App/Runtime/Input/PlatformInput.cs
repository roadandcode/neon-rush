using System;
using System.Linq;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace RoadAndCode.NeonRush.App.Input
{
    /// <summary>
    /// The input actions as this platform should see them: a private copy of the asset with every
    /// binding outside the platform's control schemes masked off. Everything that reads input
    /// receives this copy, so no code has to ask which devices are allowed.
    /// </summary>
    internal sealed class PlatformInput : IDisposable
    {
        public PlatformInput(InputActionAsset source, IInputProfile profile)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(profile, nameof(profile));

            // A copy, because the mask is runtime state and assets are never written to at runtime.
            Actions = Object.Instantiate(source);
            Actions.bindingMask = InputBinding.MaskByGroups(profile.ControlSchemes.ToArray());
        }

        public InputActionAsset Actions { get; }

        public void Dispose()
        {
            Actions.Disable();

            // Destroy only works in play mode; tests and editor tooling dispose outside it.
            if (Application.isPlaying) Object.Destroy(Actions);
            else Object.DestroyImmediate(Actions);
        }
    }
}
