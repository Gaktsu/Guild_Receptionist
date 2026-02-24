using System;
using System.IO;
using Project.Domain.Save;
using UnityEngine;

namespace Project.Systems.Save
{
    public class SaveSystem
    {
        private const string SaveFileName = "save.json";

        /// <summary>
        /// Saves <see cref="SaveGameData"/> as JSON into persistent storage.
        /// </summary>
        public void Save(SaveGameData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var path = GetSavePath();
            var directoryPath = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads save data from persistent storage. Returns null when no valid save file exists.
        /// </summary>
        public SaveGameData Load()
        {
            var path = GetSavePath();
            if (!File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var data = JsonUtility.FromJson<SaveGameData>(json);
                if (data == null)
                {
                    return null;
                }

                // Legacy migration: saves created before CurrentAP existed deserialize this int as 0.
                // When the field is missing, default AP to max-day value so users do not lose a day.
                if (!json.Contains("\"CurrentAP\"", StringComparison.Ordinal))
                {
                    data.CurrentAP = 5;
                }

                return data;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the save file when it exists.
        /// </summary>
        public void Clear()
        {
            var path = GetSavePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }
    }
}
