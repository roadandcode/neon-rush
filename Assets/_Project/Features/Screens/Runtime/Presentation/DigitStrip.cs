using RoadAndCode.Core.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// Shows a whole number as one label per digit. Each label only ever holds one of ten constant
    /// strings, so changing the number builds no string, and the score can change every frame
    /// without feeding the garbage collector. The fixed-width cells also stop the number from
    /// shifting sideways as it counts up.
    /// </summary>
    internal sealed class DigitStrip
    {
        private const string CellClass = "digit";
        private const int MaxCapacity = 9;

        private static readonly string[] Glyphs = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        // Most significant digit first, so the cells read left to right.
        private readonly Label[] _cells;
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
            _cells = new Label[capacity];
            int limit = 1;
            for (int i = 0; i < capacity; i++)
            {
                var cell = new Label { pickingMode = PickingMode.Ignore };
                cell.AddToClassList(CellClass);
                cell.SetShown(false);
                container.Add(cell);
                _cells[i] = cell;
                limit *= 10;
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
                _cells[_cells.Length - 1 - inUse].text = Glyphs[rest % 10];
                rest /= 10;
                inUse++;
            }
            while (rest > 0);

            if (inUse == _cellsInUse) return;

            // Only when the number gains or loses a digit do cells need showing or hiding.
            _cellsInUse = inUse;
            int firstInUse = _cells.Length - inUse;
            for (int i = 0; i < _cells.Length; i++) _cells[i].SetShown(i >= firstInUse);
        }
    }
}
