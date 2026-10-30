using UnityEngine;

namespace RoadAndCode.Core.Gestures
{
    /// <summary>
    /// Turns a moving contact point into swipes. Pure logic: positions in, directions out.
    ///
    /// A swipe is reported the moment the contact has travelled far enough, not when the finger
    /// lifts, because in an action game the lift comes too late. A contact reports once on each
    /// axis: however far a flick travels it is one swipe, and the next one needs a new contact.
    /// One drag can still turn a corner, sideways and then up.
    /// </summary>
    public sealed class SwipeRecognizer
    {
        private Vector2 _origin;
        private bool _reportedHorizontal;
        private bool _reportedVertical;

        public bool IsTracking { get; private set; }

        /// <summary>The contact went down at <paramref name="position"/>.</summary>
        public void Begin(Vector2 position)
        {
            _origin = position;
            _reportedHorizontal = false;
            _reportedVertical = false;
            IsTracking = true;
        }

        /// <summary>
        /// The contact moved. Returns a direction once it is at least <paramref name="threshold"/>
        /// away from the origin along its dominant axis, unless this contact has already reported
        /// on that axis. Otherwise <see cref="SwipeDirection.None"/>.
        /// </summary>
        public SwipeDirection Move(Vector2 position, float threshold)
        {
            if (!IsTracking) return SwipeDirection.None;

            Vector2 delta = position - _origin;
            float horizontal = Mathf.Abs(delta.x);
            float vertical = Mathf.Abs(delta.y);
            if (Mathf.Max(horizontal, vertical) < threshold) return SwipeDirection.None;

            // The origin keeps up with the finger either way, so the drift of a long sideways flick
            // never adds up to a swipe on the other axis.
            _origin = position;
            if (horizontal >= vertical)
            {
                if (_reportedHorizontal) return SwipeDirection.None;

                _reportedHorizontal = true;
                return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
            }

            if (_reportedVertical) return SwipeDirection.None;

            _reportedVertical = true;
            return delta.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
        }

        /// <summary>The contact lifted.</summary>
        public void End()
        {
            IsTracking = false;
        }
    }
}
