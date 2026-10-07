namespace RoadAndCode.NeonRush.Player.Logic
{
    /// <summary>Where player actions come from: a device, a touch gesture, a recording, a test.</summary>
    internal interface IPlayerActionSource
    {
        /// <summary>While disabled, nothing is queued and anything already queued is dropped.</summary>
        void SetEnabled(bool enabled);

        bool TryDequeue(out PlayerAction action);
    }
}
