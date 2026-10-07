using RoadAndCode.Core.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// Shows a whole number as a row of reels, like an odometer. Every cell holds the ten digits
    /// stacked in a column and clips all but one; changing the number slides each column to the
    /// right digit.
    ///
    /// It is built this way because the score changes many times a second for the whole of a run
    /// and nothing on that path may allocate. Building a string obviously would, but so does UI Toolkit itself
    /// whenever a label's text changes, even to a cached string: measured on a live panel, about
    /// 360 bytes per change. Moving an element allocates nothing. The only allocation left is
    /// when the number gains or loses a digit and a cell is shown or hidden, a handful of times
    /// in a run.
    /// </summary>
    internal sealed class DigitStrip
    {
        private const string CellClass = "digit";
        private const string ReelClass = "digit__reel";
        private const string GlyphClass = "digit__glyph";
        private const int MaxCapacity = 9;
        private const int Base = 10;

        // A reel is ten glyphs tall, so one glyph is this share of its own height.
        private const float GlyphShare = 100f / Base;

        private static readonly string[] Glyphs = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        // Most significant digit first, so the cells read left to right.
        private readonly VisualElement[] _cells;
        private readonly VisualElement[] _reels;
        private readonly int[] _digits;
        private readonly int _largest;
        private int _shown = -1;
        private int _cellsInUse;

        /// <param name="container">Emptied and filled with the digit cells.</param>
        /// <param name="capacity">The most digits this strip can show. Larger numbers show as all nines.</param>
        public DigitStrip(VisualElement container, int capacity)
        {
            Guard.NotNull(container, nameof(container));
            Guard.Require(capacity >= 1 && capacity <= MaxCapacity, "A digit strip holds between 1 and 9 digits.");

            container.Clear();
            _cells = new VisualElement[capacity];
            _reels = new VisualElement[capacity];
            _digits = new int[capacity];

            int limit = 1;
            for (int i = 0; i < capacity; i++)
            {
                _cells[i] = AddCell(container, out _reels[i]);
                limit *= Base;
            }

            _largest = limit - 1;
            Show(0);
        }

        public int Capacity => _cells.Length;

        public void Show(int value)
        {
            value = Mathf.Clamp(value, 0, _largest);
            if (value == _shown) return;
            _shown = value;

            // Fill from the right. Zero still takes one cell.
            int inUse = 0;
            int rest = value;
            do
            {
                Turn(_cells.Length - 1 - inUse, rest % Base);
                rest /= Base;
                inUse++;
            }
            while (rest > 0);

            if (inUse == _cellsInUse) return;

            // Only when the number gains or loses a digit do cells need showing or hiding.
            _cellsInUse = inUse;
            int firstInUse = _cells.Length - inUse;
            for (int i = 0; i < _cells.Length; i++) _cells[i].SetShown(i >= firstInUse);
        }

        private static VisualElement AddCell(VisualElement container, out VisualElement reel)
        {
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(CellClass);
            cell.SetShown(false);

            reel = new VisualElement { pickingMode = PickingMode.Ignore };
            reel.AddToClassList(ReelClass);
            foreach (string glyph in Glyphs)
            {
                var label = new Label(glyph) { pickingMode = PickingMode.Ignore };
                label.AddToClassList(GlyphClass);
                reel.Add(label);
            }

            cell.Add(reel);
            container.Add(cell);
            return cell;
        }

        private void Turn(int cell, int digit)
        {
            // A reel that has never been turned rests on zero, which is where _digits starts too.
            if (_digits[cell] == digit) return;

            // A percentage of the reel's own height, so no size in pixels has to be known here.
            _digits[cell] = digit;
            _reels[cell].style.translate = new Translate(0f, Length.Percent(-GlyphShare * digit));
        }
    }
}
