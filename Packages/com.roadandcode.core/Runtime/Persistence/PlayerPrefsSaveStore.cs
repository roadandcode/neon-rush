using System;
using UnityEngine;

namespace RoadAndCode.Core.Persistence
{
    /// <summary>
    /// Stores records as JSON in PlayerPrefs, which is the one backend that works the same on
    /// every platform, including the browser (IndexedDB). Records must be <c>[Serializable]</c>.
    /// </summary>
    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        private readonly string _prefix;

        public PlayerPrefsSaveStore(string prefix = "save.")
        {
            _prefix = prefix ?? string.Empty;
        }

        public bool TryLoad<T>(string key, out T value)
        {
            value = default;

            string json = PlayerPrefs.GetString(_prefix + key, null);
            if (string.IsNullOrEmpty(json)) return false;

            try
            {
                value = JsonUtility.FromJson<T>(json);
                return value != null;
            }
            catch (ArgumentException exception)
            {
                // A save written by an older build or edited by hand. Treat it as missing, but say so.
                Debug.LogWarning($"Save '{key}' could not be read and will be ignored: {exception.Message}");
                value = default;
                return false;
            }
        }

        public void Save<T>(string key, T value)
        {
            PlayerPrefs.SetString(_prefix + key, JsonUtility.ToJson(value));
            PlayerPrefs.Save();
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(_prefix + key);
            PlayerPrefs.Save();
        }
    }
}
