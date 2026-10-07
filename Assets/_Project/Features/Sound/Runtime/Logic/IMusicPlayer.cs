namespace RoadAndCode.NeonRush.Sound.Logic
{
    /// <summary>The two music loops. They always run together; the levels decide which is heard.</summary>
    internal interface IMusicPlayer
    {
        /// <summary>Levels from 0 to 1, before the bank's music volume is applied.</summary>
        void SetLevels(float menu, float run);
    }
}
