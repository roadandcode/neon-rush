namespace RoadAndCode.Core.Persistence
{
    /// <summary>
    /// Key/value storage for small serializable records (settings, high scores).
    /// A missing or unreadable entry is an expected case, so loading reports it instead of throwing.
    /// </summary>
    public interface ISaveStore
    {
        bool TryLoad<T>(string key, out T value);
        void Save<T>(string key, T value);
        void Delete(string key);
    }
}
