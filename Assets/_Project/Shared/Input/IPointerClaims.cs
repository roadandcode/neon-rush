using UnityEngine;

namespace RoadAndCode.NeonRush.Shared.Input
{
    /// <summary>
    /// Answers whether something on screen takes a pointer press for itself, such as an on-screen
    /// button. Gesture sources ask before starting a gesture, so pressing the pause button is not
    /// also the start of a swipe.
    /// </summary>
    public interface IPointerClaims
    {
        /// <param name="screenPosition">In pixels with the origin at the bottom left, as the input system reports it.</param>
        bool IsClaimed(Vector2 screenPosition);
    }
}
