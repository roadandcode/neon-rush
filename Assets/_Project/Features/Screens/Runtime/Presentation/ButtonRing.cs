using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    /// <summary>
    /// The buttons of one screen in document order, which is also reading order. Pressing one
    /// from here sends the button the same submit event UI Toolkit would, so a key press runs
    /// exactly the code a pointer press runs and no view needs a second path for it.
    /// </summary>
    internal sealed class ButtonRing
    {
        private const string FocusedClass = "focused";

        private readonly List<Button> _buttons = new List<Button>();

        public int Count => _buttons.Count;

        public void Collect(VisualElement screenRoot)
        {
            _buttons.Clear();
            screenRoot.Query<Button>().ToList(_buttons);
        }

        public void Clear() => _buttons.Clear();

        public void SetFocus(int index)
        {
            for (int i = 0; i < _buttons.Count; i++) _buttons[i].EnableInClassList(FocusedClass, i == index);
        }

        public void Press(int index)
        {
            if (index < 0 || index >= _buttons.Count) return;

            Button button = _buttons[index];
            using (NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = button;
                button.SendEvent(submit);
            }
        }
    }
}
