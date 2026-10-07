using NUnit.Framework;
using UnityEditor;

namespace RoadAndCode.NeonRush.App.Tests
{
    /// <summary>
    /// Project settings the game's behaviour depends on, pinned here so that a stray click in the
    /// Player settings fails a test instead of shipping.
    /// </summary>
    public sealed class PlayerSettingsTests
    {
        // The game pauses itself when it loses focus. If the engine stopped its own loop as well,
        // nothing would be drawn until focus came back, and in a browser a window resized in the
        // meantime shows an empty canvas where the pause screen should be.
        [Test]
        public void ThePlayer_KeepsRunningWithoutFocus()
        {
            Assert.That(PlayerSettings.runInBackground, Is.True);
        }
    }
}
