using System;
using UnityEngine;

namespace RoadAndCode.Core.Platforms
{
    /// <summary>
    /// One value per platform family, editable in the inspector. This is how a platform difference
    /// becomes data: the code asks for <c>For(platform.Kind)</c> and never branches on the platform.
    /// </summary>
    [Serializable]
    public struct PerPlatform<T>
    {
        [SerializeField] private T _desktop;
        [SerializeField] private T _mobile;
        [SerializeField] private T _web;

        public PerPlatform(T desktop, T mobile, T web)
        {
            _desktop = desktop;
            _mobile = mobile;
            _web = web;
        }

        public T For(PlatformKind kind)
        {
            switch (kind)
            {
                case PlatformKind.Mobile: return _mobile;
                case PlatformKind.Web: return _web;
                default: return _desktop;
            }
        }
    }
}
