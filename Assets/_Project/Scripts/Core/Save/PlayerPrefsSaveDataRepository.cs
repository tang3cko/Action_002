using UnityEngine;

namespace Action002.Core.Save
{
    public sealed class PlayerPrefsSaveDataRepository : ISaveDataRepository
    {
        public const int CURRENT_VERSION = 1;

        private const string SAVE_KEY = "Action002_SaveData";
        private const string LEGACY_KEY = "HasCompletedAwakeningTutorial";

        public SaveData Load()
        {
            bool hasNewKey = PlayerPrefs.HasKey(SAVE_KEY);
            bool hasLegacyKey = PlayerPrefs.HasKey(LEGACY_KEY);

            if (hasLegacyKey && !hasNewKey)
            {
                return MigrateLegacyData();
            }

            if (!hasNewKey)
            {
                return DefaultData();
            }

            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                return SelfHeal();
            }

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (System.Exception)
            {
                return SelfHeal();
            }

            if (data.Version != CURRENT_VERSION)
            {
                return SelfHeal();
            }

            return data;
        }

        public void Save(SaveData data)
        {
            data.Version = CURRENT_VERSION;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private static SaveData DefaultData()
        {
            return new SaveData { Version = CURRENT_VERSION };
        }

        private SaveData MigrateLegacyData()
        {
            var data = DefaultData();
            data.TutorialCompleted = PlayerPrefs.GetInt(LEGACY_KEY, 0) == 1;

            Save(data);
            PlayerPrefs.DeleteKey(LEGACY_KEY);
            PlayerPrefs.Save();

            return data;
        }

        private SaveData SelfHeal()
        {
            var data = DefaultData();
            Save(data);
            return data;
        }
    }
}
