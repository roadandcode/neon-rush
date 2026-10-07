using UnityEngine;

namespace RoadAndCode.Core.Gestures
{
    /// <summary>
    /// Turns a moving contact point into swipes. Pure logic: positions in, directions out.
    ///
    /// A swipe is reported the moment the contact has travelled far enough, not when the finger
    /// lifts, because in an action game the lift comes too late. After reporting, the origin moves
    /// to the current position, so one continuous drag can produce several swipes.
    /// </summary>
    public sealed class SwipeRecognizer
    {
        private Vector2 _origin;

        public bool IsTracking { get; private set; }

        /// <summary>The contact went down at <paramref name="position"/>.</summary>
        public void Begin(Vector2 position)
        {
            _origin = position;
            IsTracking = true;
        }

        /// <summary>
        /// The contact moved. Returns a direction once it is at least <paramref name="threshold"/>
        /// away from the origin along its dominant axis, otherwise <see cref="SwipeDirection.None"/>.
        /// </summary>
        public SwipeDirection Move(Vector2 position, float threshold)
        {
            if (!IsTracking) return SwipeDirection.None;

            Vector2 delta = position - _origin;
            float horizontal = Mathf.Abs(delta.x);
            float vertical = Mathf.Abs(delta.y);
            if (Mathf.Max(horizontal, vertical) < threshold) return SwipeDirection.None;

            _origin = position;
            if (horizontal >= vertical) return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
            return delta.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
        }

        /// <summary>The contact lifted.</summary>
        public void End()
        {
            IsTracking = false;
        }
    }
}
