using RoadAndCode.NeonRush.Sound.Data;

namespace RoadAndCode.NeonRush.Sound.Logic
{
    /// <summary>Plays one-shot sounds. The logic asks for cues; which clip that means is the output's business.</summary>
    internal interface ISoundPlayer
    {
        /// <param name="pitch">1 is the clip as recorded, 2 an octave up.</param>
        /// <param name="delay">Seconds to wait before it starts.</param>
        void Play(SoundCue cue, float pitch = 1f, float delay = 0f);
    }
}
