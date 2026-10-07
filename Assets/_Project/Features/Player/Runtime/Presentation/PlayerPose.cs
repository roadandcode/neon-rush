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

        public PlayerPose(float x, float height, float stature, float lean)
        {
            X = x;
            Height = height;
            Stature = stature;
            Lean = lean;
        }
    }
}
