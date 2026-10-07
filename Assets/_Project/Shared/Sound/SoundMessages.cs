namespace RoadAndCode.NeonRush.Shared.Sound
{
    public readonly struct SoundSettingChanged
    {
        public readonly bool SoundOn;

        public SoundSettingChanged(bool soundOn)
        {
            SoundOn = soundOn;
        }
    }
}
