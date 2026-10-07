using NUnit.Framework;
using RoadAndCode.Core.Gestures;
using UnityEngine;

namespace RoadAndCode.Core.Tests
{
    public sealed class SwipeRecognizerTests
    {
        private const float Threshold = 50f;

        private static readonly Vector2 Start = new Vector2(400f, 300f);

        private SwipeRecognizer _recognizer;

        [SetUp]
        public void SetUp()
        {
            _recognizer = new SwipeRecognizer();
        }

        [TestCase(60f, 0f, SwipeDirection.Right)]
        [TestCase(-60f, 0f, SwipeDirection.Left)]
        [TestCase(0f, 60f, SwipeDirection.Up)]
        [TestCase(0f, -60f, SwipeDirection.Down)]
        public void MovingPastTheThreshold_ReportsTheDirection(float dx, float dy, SwipeDirection expected)
        {
            _recognizer.Begin(Start);

            Assert.That(_recognizer.Move(Start + new Vector2(dx, dy), Threshold), Is.EqualTo(expected));
        }

        [Test]
        public void MovingLessThanTheThreshold_ReportsNothing()
        {
            _recognizer.Begin(Start);

            Assert.That(_recognizer.Move(Start + new Vector2(49f, 20f), Threshold), Is.EqualTo(SwipeDirection.None));
        }

        [Test]
        public void ADiagonalDrag_GoesToTheDominantAxis()
        {
            _recognizer.Begin(Start);

            Assert.That(_recognizer.Move(Start + new Vector2(30f, 70f), Threshold), Is.EqualTo(SwipeDirection.Up));
        }

        [Test]
        public void TheSwipeIsReportedOnce_ThenNeedsAnotherFullThreshold()
        {
            _recognizer.Begin(Start);
            _recognizer.Move(Start + new Vector2(60f, 0f), Threshold);

            Assert.That(_recognizer.Move(Start + new Vector2(80f, 0f), Threshold), Is.EqualTo(SwipeDirection.None));
            Assert.That(_recognizer.Move(Start + new Vector2(115f, 0f), Threshold), Is.EqualTo(SwipeDirection.Right));
        }

        [Test]
        public void OneDrag_CanChangeDirectionWithoutLifting()
        {
            _recognizer.Begin(Start);
            var first = _recognizer.Move(Start + new Vector2(60f, 0f), Threshold);
            var second = _recognizer.Move(Start + new Vector2(60f, 70f), Threshold);

            Assert.That(first, Is.EqualTo(SwipeDirection.Right));
            Assert.That(second, Is.EqualTo(SwipeDirection.Up));
        }

        [Test]
        public void MovesBeforeAContactOrAfterItLifts_AreIgnored()
        {
            Assert.That(_recognizer.Move(Start + new Vector2(200f, 0f), Threshold), Is.EqualTo(SwipeDirection.None));

            _recognizer.Begin(Start);
            _recognizer.End();

            Assert.That(_recognizer.IsTracking, Is.False);
            Assert.That(_recognizer.Move(Start + new Vector2(200f, 0f), Threshold), Is.EqualTo(SwipeDirection.None));
        }

        [Test]
        public void ANewContact_StartsFromItsOwnPosition()
        {
            _recognizer.Begin(Start);
            _recognizer.End();
            _recognizer.Begin(Start + new Vector2(300f, 0f));

            Assert.That(_recognizer.Move(Start + new Vector2(320f, 0f), Threshold), Is.EqualTo(SwipeDirection.None));
        }
    }
}
