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
        /// Loads save data from persistent storage. Returns null when no save file exists.
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

            return JsonUtility.FromJson<SaveGameData>(json);
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
