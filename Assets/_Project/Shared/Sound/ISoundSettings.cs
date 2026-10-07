namespace RoadAndCode.NeonRush.Shared.Sound
{
    /// <summary>The player's sound preference. The sound feature owns and saves it; screens show and change it.</summary>
    public interface ISoundSettings
    {
        bool SoundOn { get; }

        void SetSoundOn(bool on);
    }
}
