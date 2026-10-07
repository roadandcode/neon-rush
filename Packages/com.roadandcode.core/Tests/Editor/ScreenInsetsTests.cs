using NUnit.Framework;
using RoadAndCode.Core.Platforms;
using UnityEngine;

namespace RoadAndCode.Core.Tests
{
    public sealed class ScreenInsetsTests
    {
        private const float Width = 2400f;
        private const float Height = 1080f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void AScreenWithNoCutOut_HasNoInsets()
        {
            var insets = ScreenInsets.From(new Rect(0f, 0f, Width, Height), Width, Height);

            Assert.That(insets.Left, Is.Zero);
            Assert.That(insets.Right, Is.Zero);
            Assert.That(insets.Top, Is.Zero);
            Assert.That(insets.Bottom, Is.Zero);
        }

        [Test]
        public void ANotchOnTheLeft_InsetsOnlyTheLeft()
        {
            var insets = ScreenInsets.From(new Rect(120f, 0f, Width - 120f, Height), Width, Height);

            Assert.That(insets.Left, Is.EqualTo(0.05f).Within(Tolerance));
            Assert.That(insets.Right, Is.Zero);
            Assert.That(insets.Top, Is.Zero);
            Assert.That(insets.Bottom, Is.Zero);
        }

        [Test]
        public void TheRectangleIsMeasuredFromTheBottomLeft()
        {
            // 54 px lost at the bottom (a gesture bar), 108 px at the top, 240 px on the right.
            var insets = ScreenInsets.From(new Rect(0f, 54f, Width - 240f, Height - 54f - 108f), Width, Height);

            Assert.That(insets.Bottom, Is.EqualTo(0.05f).Within(Tolerance));
            Assert.That(insets.Top, Is.EqualTo(0.1f).Within(Tolerance));
            Assert.That(insets.Right, Is.EqualTo(0.1f).Within(Tolerance));
            Assert.That(insets.Left, Is.Zero);
        }

        [Test]
        public void ARectangleLargerThanTheScreen_IsNotANegativeInset()
        {
            var insets = ScreenInsets.From(new Rect(-10f, -10f, Width + 20f, Height + 20f), Width, Height);

            Assert.That(insets.Left, Is.Zero);
            Assert.That(insets.Right, Is.Zero);
            Assert.That(insets.Top, Is.Zero);
            Assert.That(insets.Bottom, Is.Zero);
        }

        [Test]
        public void AScreenWithNoSizeYet_HasNoInsets()
        {
            var insets = ScreenInsets.From(new Rect(0f, 0f, 0f, 0f), 0f, 0f);

            Assert.That(insets.Left + insets.Right + insets.Top + insets.Bottom, Is.Zero);
        }
    }
}
