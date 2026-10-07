namespace RoadAndCode.NeonRush.Player.Presentation
{
    /// <summary>Everything the view needs to draw the runner this frame.</summary>
    public readonly struct PlayerPose
    {
        public readonly float X;
        public readonly float Height;

        /// <summary>Body height as a fraction of standing height: 1 upright, less while sliding.</summary>
        public readonly float Stature;

        /// <summary>Roll in degrees, positive leaning right.</summary>
        public readonly float Lean;

        /// <summary>Pitch in degrees, positive tipping backwards. Zero unless the runner has been knocked down.</summary>
        public readonly float Tilt;

        public PlayerPose(float x, float height, float stature, float lean, float tilt)
        {
            X = x;
            Height = height;
            Stature = stature;
            Lean = lean;
            Tilt = tilt;
        }
    }
}
