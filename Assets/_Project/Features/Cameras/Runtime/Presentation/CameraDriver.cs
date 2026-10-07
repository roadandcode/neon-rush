using RoadAndCode.Core.Diagnostics;
using RoadAndCode.NeonRush.Cameras.Logic;
using UnityEngine;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Cameras.Presentation
{
    /// <summary>
    /// Advances the director once a frame and hands its pose to the rig. It runs from the player
    /// loop rather than the simulation, because the camera keeps moving on the menu, through the
    /// intro and after a crash, when the simulation is stopped. Late in the frame, so the camera
    /// follows whatever moved before it.
    /// </summary>
    internal sealed class CameraDriver : IStartable, ILateTickable
    {
        private readonly CameraDirector _director;
        private readonly CameraRigView _rig;

        public CameraDriver(CameraDirector director, CameraRigView rig)
        {
            _director = Guard.NotNull(director, nameof(director));
            _rig = Guard.NotNull(rig, nameof(rig));
        }

        public void Start() => _rig.Apply(_director.Pose(_rig.Aspect));

        public void LateTick()
        {
            _director.Tick(Time.deltaTime);
            _rig.Apply(_director.Pose(_rig.Aspect));
        }
    }
}
