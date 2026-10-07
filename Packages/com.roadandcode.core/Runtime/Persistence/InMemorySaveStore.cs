using System.Collections.Generic;

namespace RoadAndCode.Core.Persistence
{
    /// <summary>Keeps records for the lifetime of the object. For tests and for running without storage.</summary>
    public sealed class InMemorySaveStore : ISaveStore
    {
        private readonly Dictionary<string, object> _records = new Dictionary<string, object>();

        public bool TryLoad<T>(string key, out T value)
        {
            if (_records.TryGetValue(key, out var stored) && stored is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        public void Save<T>(string key, T value) => _records[key] = value;

        public void Delete(string key) => _records.Remove(key);
    }
}
