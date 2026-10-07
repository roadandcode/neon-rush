using RoadAndCode.NeonRush.Track.Data;

namespace RoadAndCode.NeonRush.Track.Presentation
{
    /// <summary>The prefabs that draw track entities, already loaded. Where they came from is not the presenter's concern.</summary>
    internal interface ITrackViewCatalog
    {
        /// <summary>Null for an entity that has nothing to show.</summary>
        TrackEntityView PrefabFor(TrackEntityDefinition definition);
    }
}
