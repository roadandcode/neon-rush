using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadAndCode.NeonRush.Shared.Input
{
    /// <summary>One platform's input set-up. There is one of these per platform family.</summary>
    [CreateAssetMenu(menuName = "Neon Rush/Input Profile", fileName = "InputProfile")]
    public sealed class InputProfileAsset : ScriptableObject, IInputProfile
    {
        [Tooltip("Control schemes active on this platform. Bindings in any other scheme are switched off.")]
        [SerializeField] private string[] _controlSchemes = { InputNames.Schemes.Keyboard, InputNames.Schemes.Gamepad };

        public IReadOnlyList<string> ControlSchemes => _controlSchemes;

        public bool Uses(string controlScheme) => Array.IndexOf(_controlSchemes, controlScheme) >= 0;

        private void OnValidate()
        {
            if (_controlSchemes.Length == 0) Debug.LogError($"{name}: an input profile needs at least one control scheme.", this);
        }
    }
}
