using System;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Lifetime;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Cameras.Data;
using RoadAndCode.NeonRush.Shared.Flow;
using RoadAndCode.NeonRush.Shared.Track;
using UnityEngine;

namespace RoadAndCode.NeonRush.Cameras.Logic
{
    /// <summary>
    /// Decides which shot the camera is on and how it gets from one to the next. It reacts to
    /// what the game announces and holds no reference to a camera, so the whole of the title
    /// screen's opening move can be stepped through in a test.
    /// </summary>
    internal sealed class CameraDirector : IDisposable
    {
        // The runner's home: the middle lane at the track's origin. Shots orbit this point.
        private static readonly Vector3 Subject = Vector3.zero;

        private readonly CameraSettings _settings;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private CameraShot _from;
        private CameraShot _to;
        private float _moveSeconds;
        private float _moveElapsed;
        private float _shakeLeft;
        private float _clock;

        public CameraDirector(CameraSettings settings, IGameFlow flow, ISubscriber subscriber)
        {
            _settings = Guard.NotNull(settings, nameof(settings));
            Guard.NotNull(flow, nameof(flow));
            Guard.NotNull(subscriber, nameof(subscriber));

            subscriber.Subscribe<GamePhaseChanged>(OnPhaseChanged).AddTo(_subscriptions);
            subscriber.Subscribe<RunIntroStarted>(OnIntroStarted).AddTo(_subscriptions);
            subscriber.Subscribe<HazardHit>(OnHazardHit).AddTo(_subscriptions);

            CutTo(IsOnTheTrack(flow.Phase) ? _settings.Run : _settings.Menu);
        }

        public bool IsMoving => _moveSeconds > 0f;

        /// <summary>The shot right now, part-way between two shots while the camera is moving.</summary>
        public CameraShot Shot => IsMoving ? CameraShot.Lerp(_from, _to, Ease(_moveElapsed / _moveSeconds)) : _to;

        public void Tick(float deltaTime)
        {
            _clock += deltaTime;
            _shakeLeft = Mathf.Max(0f, _shakeLeft - deltaTime);

            if (!IsMoving) return;

            _moveElapsed += deltaTime;
            if (_moveElapsed >= _moveSeconds) CutTo(_to);
        }

        public CameraPose Pose(float aspect)
        {
            CameraPose pose = ShotSolver.Solve(Shot, Subject, aspect);
            return _shakeLeft > 0f ? Shaken(pose) : pose;
        }

        public void Dispose() => _subscriptions.Dispose();

        private static bool IsOnTheTrack(GamePhase phase)
        {
            return phase == GamePhase.Run || phase == GamePhase.Paused || phase == GamePhase.GameOver;
        }

        // Slow out of the first shot and slow into the second, but not so slow out that pressing
        // play seems to do nothing for a moment.
        private static float Ease(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        private void OnIntroStarted(RunIntroStarted message) => MoveTo(_settings.Run, message.Duration);

        private void OnPhaseChanged(GamePhaseChanged message)
        {
            if (message.Current == GamePhase.Menu)
            {
                // The game opens on the menu shot; after a run the camera travels back to it.
                if (message.Previous == GamePhase.Boot) CutTo(_settings.Menu);
                else MoveTo(_settings.Menu, _settings.ReturnSeconds);
                return;
            }

            // A run that starts without an intro has no move to carry the camera there.
            if (message.Current == GamePhase.Run && message.Previous == GamePhase.Menu) CutTo(_settings.Run);
        }

        private void OnHazardHit(HazardHit message) => _shakeLeft = _settings.ShakeSeconds;

        private void CutTo(in CameraShot shot)
        {
            _from = shot;
            _to = shot;
            _moveSeconds = 0f;
            _moveElapsed = 0f;
        }

        private void MoveTo(in CameraShot shot, float seconds)
        {
            if (seconds <= 0f)
            {
                CutTo(shot);
                return;
            }

            // Start from wherever the camera is, so a move that interrupts another does not jump.
            _from = Shot;
            _to = shot;
            _moveSeconds = seconds;
            _moveElapsed = 0f;
        }

        // Two sine waves at unrelated rates, one per screen axis, fading out with the square of
        // the time left: a hard jolt that settles quickly, and the same every time.
        private CameraPose Shaken(in CameraPose pose)
        {
            float left = _shakeLeft / _settings.ShakeSeconds;
            float reach = _settings.ShakeDistance * left * left;
            float phase = _clock * _settings.ShakeFrequency * 2f * Mathf.PI;

            var offset = new Vector3(Mathf.Sin(phase) * reach, Mathf.Sin(phase * 1.37f + 1.3f) * reach, 0f);
            return new CameraPose(pose.Position + pose.Rotation * offset, pose.Rotation, pose.FieldOfView);
        }
    }
}
