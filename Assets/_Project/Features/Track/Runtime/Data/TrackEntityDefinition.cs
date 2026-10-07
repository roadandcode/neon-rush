using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Track.Logic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoadAndCode.NeonRush.Track.Data
{
    /// <summary>
    /// Shared shape of every track entity asset: what it looks like and the box it occupies.
    /// The box is what the runner collides with, so "jump over" and "slide under" are just boxes
    /// that leave room above or below.
    ///
    /// The look is a reference, not the prefab itself. These assets are small and always in
    /// memory because patterns point at them; the prefabs, with their meshes and materials, are
    /// loaded through Addressables and can be shipped, swapped or added to separately.
    /// </summary>
    internal abstract class TrackEntityDefinition : ScriptableObject, ITrackEntityDefinition
    {
        [Tooltip("The prefab that draws it. Must be in an Addressables group and carry a TrackEntityView.")]
        [SerializeField] private AssetReferenceGameObject _view;

        [Tooltip("Width, height and depth of the collision box, in metres.")]
        [SerializeField] private Vector3 _size = Vector3.one;

        [Tooltip("Height of the underside of the box above the track. Above zero means it can be slid under.")]
        [SerializeField, Min(0f)] private float _bottom;

        [SerializeField] private float _spinDegreesPerSecond;

        public AssetReferenceGameObject View => _view;

        public Vector3 Size => _size;

        public float Bottom => _bottom;

        public float Top => _bottom + _size.y;

        public float SpinDegreesPerSecond => _spinDegreesPerSecond;

        public Bounds BoundsAt(float x, float z)
        {
            return new Bounds(new Vector3(x, _bottom + _size.y * 0.5f, z), _size);
        }

        public abstract bool OnTouched(IPublisher publisher);

        protected virtual void OnValidate()
        {
            if (_view == null || !_view.RuntimeKeyIsValid()) Debug.LogError($"{name}: no view assigned.", this);
        }
    }
}
