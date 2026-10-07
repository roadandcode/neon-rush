using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.StateMachines;
using RoadAndCode.NeonRush.Player.Data;
using RoadAndCode.NeonRush.Shared.Run;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Player.Logic
{
    /// <summary>
    /// The runner's movement rules: which lane, how high, how tall. Pure logic with no scene
    /// objects, so every rule here is unit-tested.
    /// </summary>
    internal sealed class PlayerMotor : IRunnerBody
    {
        private readonly PlayerTuning _tuning;
        private readonly ILaneLayout _lanes;
        private readonly StateMachine<PlayerMode> _mode;

        private float _modeTime;
        private bool _diving;

        public PlayerMotor(PlayerTuning tuning, ILaneLayout lanes)
        {
            _tuning = Guard.NotNull(tuning, nameof(tuning));
            _lanes = Guard.NotNull(lanes, nameof(lanes));

            _mode = new StateMachine<PlayerMode>()
                .AddState(PlayerMode.Running)
                .AddState(PlayerMode.Jumping, new Jumping(this))
                .AddState(PlayerMode.Sliding, new Sliding(this))
                .AddState(PlayerMode.Down)
                .Allow(PlayerMode.Running, PlayerMode.Jumping)
                .Allow(PlayerMode.Running, PlayerMode.Sliding)
                .Allow(PlayerMode.Jumping, PlayerMode.Running)
                .Allow(PlayerMode.Jumping, PlayerMode.Sliding)
                .Allow(PlayerMode.Sliding, PlayerMode.Running)
                .Allow(PlayerMode.Sliding, PlayerMode.Jumping)
                .Allow(PlayerMode.Down, PlayerMode.Running)
                .AllowFromAny(PlayerMode.Down);

            Lane = _lanes.CenterLane;
            X = _lanes.CenterOf(Lane);
            _mode.Start(PlayerMode.Running);
        }

        public PlayerMode Mode => _mode.Current;

        /// <summary>The lane the runner is in or heading to.</summary>
        public int Lane { get; private set; }

        /// <summary>Sideways position in metres. Trails <see cref="Lane"/> while changing lanes.</summary>
        public float X { get; private set; }

        /// <summary>Height of the feet above the track.</summary>
        public float Height { get; private set; }

        public float TargetX => _lanes.CenterOf(Lane);

        public float BodyHeight => Mode == PlayerMode.Sliding ? _tuning.SlidingHeight : _tuning.StandingHeight;

        public Bounds Bounds
        {
            get
            {
                float bodyHeight = BodyHeight;
                return new Bounds(
                    new Vector3(X, Height + bodyHeight * 0.5f, 0f),
                    new Vector3(_tuning.Width, bodyHeight, _tuning.Depth));
            }
        }

        /// <summary>Back to the middle lane, on the ground, running.</summary>
        public void Reset()
        {
            Lane = _lanes.CenterLane;
            X = TargetX;
            Height = 0f;
            _diving = false;
            if (Mode != PlayerMode.Running) _mode.Go(PlayerMode.Running);
        }

        /// <summary>Applies an action if the rules allow it right now. Returns whether it took effect.</summary>
        public bool Apply(PlayerAction action)
        {
            if (Mode == PlayerMode.Down) return false;

            switch (action)
            {
                case PlayerAction.MoveLeft: return TryShiftLane(-1);
                case PlayerAction.MoveRight: return TryShiftLane(1);
                case PlayerAction.Jump: return _mode.TryGo(PlayerMode.Jumping);
                case PlayerAction.Slide: return TrySlide();
                default: return false;
            }
        }

        public void KnockDown() => _mode.TryGo(PlayerMode.Down);

        public void Tick(float deltaTime)
        {
            if (Mode == PlayerMode.Down) return;

            X = Mathf.MoveTowards(X, TargetX, _tuning.LaneChangeSpeed * deltaTime);
            _mode.Tick(deltaTime);
        }

        private bool TryShiftLane(int direction)
        {
            int target = Lane + direction;
            if (!_lanes.Contains(target)) return false;

            Lane = target;
            return true;
        }

        private bool TrySlide()
        {
            if (Mode != PlayerMode.Jumping) return _mode.TryGo(PlayerMode.Sliding);

            // Slide pressed in the air: drop fast and go straight into the slide on landing.
            if (_diving) return false;
            _diving = true;
            return true;
        }

        /// <summary>A fixed-length arc, so jump distance scales with run speed and timing stays learnable.</summary>
        private sealed class Jumping : State
        {
            private readonly PlayerMotor _motor;

            public Jumping(PlayerMotor motor)
            {
                _motor = motor;
            }

            public override void Enter()
            {
                _motor._modeTime = 0f;
                _motor._diving = false;
            }

            public override void Tick(float deltaTime)
            {
                var tuning = _motor._tuning;

                if (_motor._diving)
                {
                    _motor.Height -= tuning.DiveSpeed * deltaTime;
                    if (_motor.Height <= 0f) Land(PlayerMode.Sliding);
                    return;
                }

                _motor._modeTime += deltaTime;
                float t = _motor._modeTime / tuning.JumpDuration;
                if (t >= 1f)
                {
                    Land(PlayerMode.Running);
                    return;
                }

                _motor.Height = 4f * tuning.JumpHeight * t * (1f - t);
            }

            private void Land(PlayerMode next)
            {
                _motor.Height = 0f;
                _motor._diving = false;
                _motor._mode.Go(next);
            }
        }

        private sealed class Sliding : State
        {
            private readonly PlayerMotor _motor;

            public Sliding(PlayerMotor motor)
            {
                _motor = motor;
            }

            public override void Enter() => _motor._modeTime = 0f;

            public override void Tick(float deltaTime)
            {
                _motor._modeTime += deltaTime;
                if (_motor._modeTime >= _motor._tuning.SlideDuration) _motor._mode.Go(PlayerMode.Running);
            }
        }
    }
}
