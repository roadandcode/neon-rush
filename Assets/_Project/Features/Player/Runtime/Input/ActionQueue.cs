using RoadAndCode.NeonRush.Player.Logic;

namespace RoadAndCode.NeonRush.Player.Input
{
    /// <summary>
    /// Input arrives as callbacks between ticks, so each source parks its actions here until the
    /// simulation asks for them. Fixed size: no allocation, and a stuck key can't grow it.
    /// </summary>
    internal sealed class ActionQueue
    {
        private const int Capacity = 8;

        private readonly PlayerAction[] _items = new PlayerAction[Capacity];
        private int _head;
        private int _count;

        public int Count => _count;

        /// <summary>Returns false, dropping the action, when the queue is full.</summary>
        public bool Enqueue(PlayerAction action)
        {
            if (_count == Capacity) return false;

            _items[(_head + _count) % Capacity] = action;
            _count++;
            return true;
        }

        public bool TryDequeue(out PlayerAction action)
        {
            if (_count == 0)
            {
                action = default;
                return false;
            }

            action = _items[_head];
            _head = (_head + 1) % Capacity;
            _count--;
            return true;
        }

        public void Clear()
        {
            _head = 0;
            _count = 0;
        }
    }
}
