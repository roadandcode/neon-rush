using System;
using UnityEngine.UIElements;

namespace RoadAndCode.NeonRush.Screens.Presentation
{
    internal static class ElementExtensions
    {
        private const string HiddenClass = "hidden";

        /// <summary>
        /// Finds a named element or fails with the name in the message. A view's element names are
        /// a contract with the UXML, and a renamed element should stop the game at start-up, not
        /// surface as a null reference the first time a button is used.
        /// </summary>
        public static T Require<T>(this VisualElement root, string name) where T : VisualElement
        {
            T element = root.Q<T>(name);
            if (element == null) throw new InvalidOperationException($"The UI document has no {typeof(T).Name} named '{name}'.");
            return element;
        }

        /// <summary>Hidden elements are taken out of layout, so they cost nothing to draw and take no input.</summary>
        public static void SetShown(this VisualElement element, bool shown) => element.EnableInClassList(HiddenClass, !shown);
    }
}
