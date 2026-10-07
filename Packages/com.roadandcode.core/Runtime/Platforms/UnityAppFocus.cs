using System;
using UnityEngine;

namespace RoadAndCode.Core.Platforms
{
    public sealed class UnityAppFocus : IAppFocus, IDisposable
    {
        public UnityAppFocus()
        {
            Application.focusChanged += OnFocusChanged;
        }

        public event Action<bool> Changed;

        public bool HasFocus => Application.isFocused;

        public void Dispose() => Application.focusChanged -= OnFocusChanged;

        private void OnFocusChanged(bool focused) => Changed?.Invoke(focused);
    }
}
