using System.Linq;
using NUnit.Framework;
using RoadAndCode.NeonRush.Screens.Presentation;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using UnityEngine.UIElements;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class DigitStripTests
    {
        private const string HiddenClass = "hidden";
        private const float PercentPerDigit = 10f;

        private VisualElement _container;
        private DigitStrip _strip;

        [SetUp]
        public void SetUp()
        {
            _container = new VisualElement();
            _strip = new DigitStrip(_container, capacity: 5);
        }

        private static VisualElement ReelOf(VisualElement cell) => cell[0];

        /// <summary>The digit a cell's window is over: how many glyphs up its reel has been slid.</summary>
        private static int DigitOn(VisualElement cell)
        {
            StyleTranslate slide = ReelOf(cell).style.translate;
            float percent = slide.keyword == StyleKeyword.Null ? 0f : slide.value.y.value;
            return Mathf.RoundToInt(-percent / PercentPerDigit);
        }

        /// <summary>What a player would read: the digits of the cells that are showing, left to right.</summary>
        private string Shown()
        {
            return string.Concat(_container.Children()
                .Where(cell => !cell.ClassListContains(HiddenClass))
                .Select(cell => DigitOn(cell).ToString()));
        }

        [Test]
        public void ANewStrip_ShowsZero()
        {
            Assert.That(Shown(), Is.EqualTo("0"));
            Assert.That(_container.childCount, Is.EqualTo(5));
        }

        [TestCase(7, "7")]
        [TestCase(10, "10")]
        [TestCase(1234, "1234")]
        [TestCase(40005, "40005")]
        [TestCase(99999, "99999")]
        public void ANumber_IsShownDigitByDigit(int value, string expected)
        {
            _strip.Show(value);

            Assert.That(Shown(), Is.EqualTo(expected));
        }

        [Test]
        public void ASmallerNumber_HidesTheCellsItNoLongerNeeds()
        {
            _strip.Show(12345);

            _strip.Show(67);

            Assert.That(Shown(), Is.EqualTo("67"));
        }

        [Test]
        public void ACellThatComesBackIntoUse_ShowsItsNewDigit_NotItsOldOne()
        {
            _strip.Show(90000);
            _strip.Show(5);

            _strip.Show(30000);

            Assert.That(Shown(), Is.EqualTo("30000"));
        }

        [Test]
        public void ANumberTooLongForTheStrip_ShowsAsAllNines()
        {
            _strip.Show(1234567);

            Assert.That(Shown(), Is.EqualTo("99999"));
        }

        [Test]
        public void ANegativeNumber_ShowsAsZero()
        {
            _strip.Show(-5);

            Assert.That(Shown(), Is.EqualTo("0"));
        }

        // The whole point of the reel: on a live panel, changing a label's text allocates.
        [Test]
        public void EveryReel_HoldsTheTenDigitsInOrder_AndTheirTextNeverChanges()
        {
            string[] Texts() => _container.Query<Label>().ToList().Select(label => label.text).ToArray();
            string[] before = Texts();

            _strip.Show(98765);
            _strip.Show(1234);

            Assert.That(before, Has.Length.EqualTo(50));
            Assert.That(before.Take(10), Is.EqualTo(new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }));
            Assert.That(Texts(), Is.EqualTo(before));
        }

        [Test]
        public void NothingInTheStrip_TakesPointerInput()
        {
            var everything = _container.Query<VisualElement>().ToList().Where(element => element != _container);

            Assert.That(everything.All(element => element.pickingMode == PickingMode.Ignore), Is.True);
        }

        [Test]
        public void TheContainer_IsEmptiedFirst()
        {
            var container = new VisualElement();
            container.Add(new Label("left over"));

            _ = new DigitStrip(container, capacity: 3);

            Assert.That(container.childCount, Is.EqualTo(3));
        }

        [Test]
        public void CountingUp_AllocatesNothing()
        {
            // Touch every digit and every length once, as a run would in its first seconds.
            for (int i = 0; i <= 99999; i += 1111) _strip.Show(i);
            _strip.Show(0);

            Assert.That(() =>
            {
                for (int i = 0; i < 2000; i++) _strip.Show(i * 7);
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
