namespace RoadAndCode.NeonRush.Sound.Data
{
    /// <summary>
    /// Every one-shot sound the game can ask for. The values are saved in the sound bank asset,
    /// so new cues are added at the end and existing ones keep their numbers.
    /// </summary>
    internal enum SoundCue
    {
        ButtonPress = 0,
        IntroSwoosh = 1,
        LaneChange = 2,
        Jump = 3,
        Slide = 4,
        Pickup = 5,
        Crash = 6,
        NewBest = 7,
    }
}
