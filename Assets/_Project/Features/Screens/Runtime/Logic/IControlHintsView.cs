namespace RoadAndCode.NeonRush.Screens.Logic
{
    internal interface IControlHintsView
    {
        /// <summary>Turns on the hints written for one control scheme. Hints for other schemes stay hidden.</summary>
        void ShowHintsFor(string controlScheme);
    }
}
