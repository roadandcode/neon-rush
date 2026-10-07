using System;
using UnityEngine;

namespace RoadAndCode.Core.Platforms
{
    /// <summary>
    /// How far the usable part of the screen is set in from each edge, as a share of the screen's
    /// width (left, right) or height (top, bottom). All zero on a screen with no notch or cut-out.
    /// Shares rather than pixels, so the same value works in any UI scale.
    /// </summary>
    public readonly struct ScreenInsets : IEquatable<ScreenInsets>
    {
        public readonly float Left;
        public readonly float Right;
        public readonly float Top;
        public readonly float Bottom;

        public ScreenInsets(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        /// <param name="safeArea">The usable rectangle in pixels with its origin at the bottom left, as Unity reports it.</param>
        public static ScreenInsets From(Rect safeArea, float screenWidth, float screenHeight)
        {
            // A zero-sized screen is reported for a frame or two while a window is being created.
            if (screenWidth <= 0f || screenHeight <= 0f) return default;

            return new ScreenInsets(
                Mathf.Clamp01(safeArea.xMin / screenWidth),
                Mathf.Clamp01((screenWidth - safeArea.xMax) / screenWidth),
                Mathf.Clamp01((screenHeight - safeArea.yMax) / screenHeight),
                Mathf.Clamp01(safeArea.yMin / screenHeight));
        }

        // Exact comparison on purpose: this is used to notice that the screen reported something new.
        public bool Equals(ScreenInsets other)
        {
            return Left == other.Left && Right == other.Right && Top == other.Top && Bottom == other.Bottom;
        }

        public override bool Equals(object obj) => obj is ScreenInsets other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Left, Right, Top, Bottom);
    }
}
