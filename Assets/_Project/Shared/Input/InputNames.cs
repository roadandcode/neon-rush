namespace RoadAndCode.NeonRush.Shared.Input
{
    /// <summary>Names used in the input actions asset, in one place.</summary>
    public static class InputNames
    {
        /// <summary>
        /// Control schemes: one per device family. Every binding belongs to exactly one, which is
        /// what lets a platform's input profile switch whole families on or off.
        /// </summary>
        public static class Schemes
        {
            public const string Keyboard = "Keyboard";
            public const string Gamepad = "Gamepad";

            /// <summary>Touch, mouse and pen: anything that presses and moves on the screen.</summary>
            public const string Pointer = "Pointer";
        }

        /// <summary>Button actions for the runner.</summary>
        public static class Run
        {
            public const string Map = "Run";
            public const string MoveLeft = "MoveLeft";
            public const string MoveRight = "MoveRight";
            public const string Jump = "Jump";
            public const string Slide = "Slide";
        }

        /// <summary>Button actions for the game flow.</summary>
        public static class Flow
        {
            public const string Map = "Flow";
            public const string Pause = "Pause";
        }

        /// <summary>Button actions for moving between on-screen controls and pressing one.</summary>
        public static class Menu
        {
            public const string Map = "Menu";
            public const string Previous = "Previous";
            public const string Next = "Next";
            public const string Submit = "Submit";
        }

        /// <summary>Raw contact state, turned into gestures by a gesture source.</summary>
        public static class Pointer
        {
            public const string Map = "Pointer";
            public const string Press = "Press";
            public const string Position = "Position";
        }
    }
}
