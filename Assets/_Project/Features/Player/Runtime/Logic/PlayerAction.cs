namespace RoadAndCode.NeonRush.Player.Logic
{
    /// <summary>
    /// What the player asked for, independent of which device asked. Because actions are plain
    /// values, they can be queued, buffered, recorded and replayed.
    /// </summary>
    internal enum PlayerAction
    {
        MoveLeft,
        MoveRight,
        Jump,
        Slide,
    }

    internal enum PlayerMode
    {
        Running,
        Jumping,
        Sliding,
        Down,
    }
}
