using System;
using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;

namespace RoadAndCode.Core.StateMachines
{
    /// <summary>
    /// Finite state machine keyed by a value (normally an enum). Transitions have to be declared
    /// up front, so an illegal change is rejected instead of silently corrupting the flow.
    /// A state may carry an <see cref="IState"/> behaviour, or none if only the key matters.
    /// </summary>
    public sealed class StateMachine<TKey>
    {
        private readonly Dictionary<TKey, IState> _behaviours = new Dictionary<TKey, IState>();
        private readonly HashSet<(TKey From, TKey To)> _transitions = new HashSet<(TKey, TKey)>();
        private readonly HashSet<TKey> _reachableFromAny = new HashSet<TKey>();
        private readonly Queue<TKey> _pending = new Queue<TKey>();
        private readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

        private IState _currentBehaviour;
        private bool _started;
        private bool _changing;

        /// <summary>Raised after a change completes: (previous, current).</summary>
        public event Action<TKey, TKey> Changed;

        public TKey Current { get; private set; }

        public bool IsIn(TKey key) => _started && _comparer.Equals(Current, key);

        public StateMachine<TKey> AddState(TKey key, IState behaviour = null)
        {
            Guard.Require(!_behaviours.ContainsKey(key), $"State '{key}' is already registered.");
            _behaviours.Add(key, behaviour);
            return this;
        }

        public StateMachine<TKey> Allow(TKey from, TKey to)
        {
            RequireKnown(from);
            RequireKnown(to);
            _transitions.Add((from, to));
            return this;
        }

        /// <summary>Declares a state that can be entered from every other state (death, shutdown).</summary>
        public StateMachine<TKey> AllowFromAny(TKey to)
        {
            RequireKnown(to);
            _reachableFromAny.Add(to);
            return this;
        }

        public void Start(TKey initial)
        {
            Guard.Require(!_started, "State machine has already been started.");
            RequireKnown(initial);

            _started = true;
            Current = initial;
            _currentBehaviour = _behaviours[initial];
            _currentBehaviour?.Enter();
        }

        public bool CanGo(TKey to)
        {
            if (!_started || !_behaviours.ContainsKey(to) || _comparer.Equals(Current, to)) return false;
            return _reachableFromAny.Contains(to) || _transitions.Contains((Current, to));
        }

        /// <summary>
        /// Changes state if the transition is declared. A change requested from inside
        /// Enter or Exit is queued and applied once the current change has finished.
        /// </summary>
        public bool TryGo(TKey to)
        {
            if (_changing)
            {
                _pending.Enqueue(to);
                return true;
            }

            if (!CanGo(to)) return false;

            Apply(to);
            while (_pending.Count > 0)
            {
                // The caller was already told "queued", so an illegal one can't be reported back. Fail loudly.
                var next = _pending.Dequeue();
                Guard.Require(CanGo(next), $"Queued transition '{Current}' -> '{next}' is not allowed.");
                Apply(next);
            }

            return true;
        }

        public void Go(TKey to)
        {
            Guard.Require(TryGo(to), $"Transition '{Current}' -> '{to}' is not allowed.");
        }

        public void Tick(float deltaTime) => _currentBehaviour?.Tick(deltaTime);

        private void Apply(TKey to)
        {
            _changing = true;
            var previous = Current;
            try
            {
                _currentBehaviour?.Exit();
                Current = to;
                _currentBehaviour = _behaviours[to];
                _currentBehaviour?.Enter();
            }
            finally
            {
                _changing = false;
            }

            Changed?.Invoke(previous, to);
        }

        private void RequireKnown(TKey key)
        {
            Guard.Require(_behaviours.ContainsKey(key), $"State '{key}' has not been registered.");
        }
    }
}
