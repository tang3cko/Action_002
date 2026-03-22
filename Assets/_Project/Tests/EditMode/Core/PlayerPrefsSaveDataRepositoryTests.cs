using NUnit.Framework;
using UnityEngine;
using Action002.Core.Save;

namespace Action002.Tests.Core
{
    public class PlayerPrefsSaveDataRepositoryTests
    {
        private const string SAVE_KEY = "Action002_SaveData";
        private const string LEGACY_KEY = "HasCompletedAwakeningTutorial";

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.DeleteKey(LEGACY_KEY);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.DeleteKey(LEGACY_KEY);
            PlayerPrefs.Save();
        }

        private static PlayerPrefsSaveDataRepository CreateRepository()
        {
            return new PlayerPrefsSaveDataRepository();
        }

        #region Load - First Launch

        [Test]
        public void Load_NoKeys_ShouldReturnDefaultWithVersion1()
        {
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
            Assert.AreEqual(0, data.HighScore);
            Assert.IsFalse(data.TutorialCompleted);
        }

        [Test]
        public void Load_NoKeys_ShouldNotPersist()
        {
            var repo = CreateRepository();

            repo.Load();

            Assert.IsFalse(PlayerPrefs.HasKey(SAVE_KEY));
        }

        #endregion

        #region Load - Legacy Migration

        [Test]
        public void Load_LegacyKeyOnly_ShouldMigrateTutorialFlag()
        {
            PlayerPrefs.SetInt(LEGACY_KEY, 1);
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.IsTrue(data.TutorialCompleted);
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
        }

        [Test]
        public void Load_LegacyKeyOnly_ShouldPersistNewKeyAndDeleteLegacy()
        {
            PlayerPrefs.SetInt(LEGACY_KEY, 1);
            var repo = CreateRepository();

            repo.Load();

            Assert.IsTrue(PlayerPrefs.HasKey(SAVE_KEY));
            Assert.IsFalse(PlayerPrefs.HasKey(LEGACY_KEY));
        }

        [Test]
        public void Load_LegacyKeyZero_ShouldMigrateAsFalse()
        {
            PlayerPrefs.SetInt(LEGACY_KEY, 0);
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.IsFalse(data.TutorialCompleted);
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
            Assert.IsTrue(PlayerPrefs.HasKey(SAVE_KEY));
            Assert.IsFalse(PlayerPrefs.HasKey(LEGACY_KEY));
        }

        [Test]
        public void Load_LegacyKeyOnly_ShouldHaveZeroStats()
        {
            PlayerPrefs.SetInt(LEGACY_KEY, 1);
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(0, data.HighScore);
            Assert.AreEqual(0, data.BestCombo);
            Assert.AreEqual(0, data.TotalKills);
            Assert.AreEqual(0, data.TotalAbsorptions);
        }

        #endregion

        #region Load - Corrupted Data

        [Test]
        public void Load_EmptyJson_ShouldSelfHealWithVersion1()
        {
            PlayerPrefs.SetString(SAVE_KEY, "");
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
        }

        [Test]
        public void Load_CorruptedJson_ShouldSelfHealWithVersion1()
        {
            PlayerPrefs.SetString(SAVE_KEY, "{broken json");
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
        }

        [Test]
        public void Load_EmptyJson_ShouldPersistSelfHealedData()
        {
            PlayerPrefs.SetString(SAVE_KEY, "");
            var repo = CreateRepository();

            repo.Load();

            Assert.IsTrue(PlayerPrefs.HasKey(SAVE_KEY));
            var reloaded = repo.Load();
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, reloaded.Version);
        }

        #endregion

        #region Load - Abnormal Version

        [Test]
        public void Load_VersionZero_ShouldSelfHeal()
        {
            var abnormal = new SaveData { HighScore = 999, Version = 0 };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(abnormal));
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
            Assert.AreEqual(0, data.HighScore);
        }

        [Test]
        public void Load_VersionGreaterThanCurrent_ShouldSelfHeal()
        {
            var abnormal = new SaveData { HighScore = 999, Version = 99 };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(abnormal));
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
            Assert.AreEqual(0, data.HighScore);
        }

        [Test]
        public void Load_NegativeVersion_ShouldSelfHeal()
        {
            var abnormal = new SaveData { HighScore = 999, Version = -1 };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(abnormal));
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, data.Version);
            Assert.AreEqual(0, data.HighScore);
        }

        [Test]
        public void Load_AbnormalVersion_ShouldPersistSelfHealedData()
        {
            var abnormal = new SaveData { HighScore = 999, Version = 99 };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(abnormal));
            var repo = CreateRepository();

            repo.Load();

            var reloaded = repo.Load();
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, reloaded.Version);
            Assert.AreEqual(0, reloaded.HighScore);
        }

        #endregion

        #region Load - Normal

        [Test]
        public void Load_ValidData_ShouldDeserializeCorrectly()
        {
            var saved = new SaveData
            {
                HighScore = 5000,
                TutorialCompleted = true,
                BestCombo = 42,
                TotalKills = 300,
                TotalAbsorptions = 150,
                Version = 1
            };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(saved));
            var repo = CreateRepository();

            var data = repo.Load();

            Assert.AreEqual(5000, data.HighScore);
            Assert.IsTrue(data.TutorialCompleted);
            Assert.AreEqual(42, data.BestCombo);
            Assert.AreEqual(300, data.TotalKills);
            Assert.AreEqual(150, data.TotalAbsorptions);
        }

        #endregion

        #region Save

        [Test]
        public void Save_ShouldPersistDataWithVersion1()
        {
            var repo = CreateRepository();
            var data = new SaveData { HighScore = 1234 };

            repo.Save(data);

            var loaded = repo.Load();
            Assert.AreEqual(1234, loaded.HighScore);
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, loaded.Version);
        }

        [Test]
        public void Save_ThenLoad_ShouldRoundTrip()
        {
            var repo = CreateRepository();
            var original = new SaveData
            {
                HighScore = 7777,
                TutorialCompleted = true,
                BestCombo = 99,
                TotalKills = 500,
                TotalAbsorptions = 250,
                Version = 1
            };

            repo.Save(original);
            var loaded = repo.Load();

            Assert.AreEqual(original.HighScore, loaded.HighScore);
            Assert.AreEqual(original.TutorialCompleted, loaded.TutorialCompleted);
            Assert.AreEqual(original.BestCombo, loaded.BestCombo);
            Assert.AreEqual(original.TotalKills, loaded.TotalKills);
            Assert.AreEqual(original.TotalAbsorptions, loaded.TotalAbsorptions);
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, loaded.Version);
        }

        [Test]
        public void Save_ShouldOverwritePreviousData()
        {
            var repo = CreateRepository();

            repo.Save(new SaveData { HighScore = 100, Version = 1 });
            repo.Save(new SaveData { HighScore = 200, Version = 1 });

            var loaded = repo.Load();
            Assert.AreEqual(200, loaded.HighScore);
        }

        [Test]
        public void Save_ShouldForceVersion1_EvenIfInputHasDifferentVersion()
        {
            var repo = CreateRepository();
            var data = new SaveData { HighScore = 100, Version = 99 };

            repo.Save(data);

            var loaded = repo.Load();
            Assert.AreEqual(PlayerPrefsSaveDataRepository.CURRENT_VERSION, loaded.Version);
        }

        [Test]
        public void Load_AfterSelfHeal_SubsequentSaveShouldWork()
        {
            PlayerPrefs.SetString(SAVE_KEY, "{broken}");
            var repo = CreateRepository();

            var data = repo.Load();
            data.HighScore = 1000;
            repo.Save(data);

            var reloaded = repo.Load();
            Assert.AreEqual(1000, reloaded.HighScore);
        }

        [Test]
        public void Load_CalledMultipleTimes_ShouldReturnSameData()
        {
            var saved = new SaveData { HighScore = 3000, Version = 1 };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(saved));
            var repo = CreateRepository();

            var first = repo.Load();
            var second = repo.Load();

            Assert.AreEqual(first.HighScore, second.HighScore);
            Assert.AreEqual(first.Version, second.Version);
        }

        #endregion
    }
}
