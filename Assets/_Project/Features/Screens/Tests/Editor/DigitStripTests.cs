using System.Linq;
using NUnit.Framework;
using RoadAndCode.NeonRush.Screens.Presentation;
using UnityEngine.TestTools.Constraints;
using UnityEngine.UIElements;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class DigitStripTests
    {
        private const string HiddenClass = "hidden";

        private VisualElement _container;
        private DigitStrip _strip;

        [SetUp]
        public void SetUp()
        {
            _container = new VisualElement();
            _strip = new DigitStrip(_container, capacity: 5);
        }

        /// <summary>What a player would read: the text of the cells that are showing, left to right.</summary>
        private string Shown()
        {
            return string.Concat(_container.Children()
                .Where(cell => !cell.ClassListContains(HiddenClass))
                .Select(cell => ((Label)cell).text));
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

        [Test]
        public void TheCells_TakeNoPointerInput()
        {
            Assert.That(_container.Children().All(cell => cell.pickingMode == PickingMode.Ignore), Is.True);
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
