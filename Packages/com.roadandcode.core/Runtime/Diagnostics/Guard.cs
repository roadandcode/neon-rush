using System;

namespace RoadAndCode.Core.Diagnostics
{
    /// <summary>Argument checks that fail at the call site instead of three frames later.</summary>
    public static class Guard
    {
        public static T NotNull<T>(T value, string name) where T : class
        {
            if (value == null) throw new ArgumentNullException(name);
            return value;
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
