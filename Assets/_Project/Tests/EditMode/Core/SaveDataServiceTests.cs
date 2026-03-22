using NUnit.Framework;
using Action002.Core.Save;

namespace Action002.Tests.Core
{
    public class SaveDataServiceTests
    {
        private FakeSaveDataRepository fakeRepo;

        [SetUp]
        public void SetUp()
        {
            fakeRepo = new FakeSaveDataRepository();
        }

        #region ApplyRunResult

        [Test]
        public void ApplyRunResult_HigherScore_ShouldUpdateHighScore()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 100, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(200, 0, 0, 0);

            Assert.AreEqual(200, fakeRepo.StoredData.HighScore);
        }

        [Test]
        public void ApplyRunResult_LowerScore_ShouldNotUpdateHighScore()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 300, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(100, 0, 0, 0);

            Assert.AreEqual(300, fakeRepo.StoredData.HighScore);
        }

        [Test]
        public void ApplyRunResult_HigherCombo_ShouldUpdateBestCombo()
        {
            fakeRepo.StoredData = new SaveData { BestCombo = 10, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 25, 0, 0);

            Assert.AreEqual(25, fakeRepo.StoredData.BestCombo);
        }

        [Test]
        public void ApplyRunResult_LowerCombo_ShouldNotUpdateBestCombo()
        {
            fakeRepo.StoredData = new SaveData { BestCombo = 50, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 10, 0, 0);

            Assert.AreEqual(50, fakeRepo.StoredData.BestCombo);
        }

        [Test]
        public void ApplyRunResult_ShouldAccumulateKills()
        {
            fakeRepo.StoredData = new SaveData { TotalKills = 100, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, 30, 0);

            Assert.AreEqual(130, fakeRepo.StoredData.TotalKills);
        }

        [Test]
        public void ApplyRunResult_ShouldAccumulateAbsorptions()
        {
            fakeRepo.StoredData = new SaveData { TotalAbsorptions = 50, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, 0, 20);

            Assert.AreEqual(70, fakeRepo.StoredData.TotalAbsorptions);
        }

        [Test]
        public void ApplyRunResult_ShouldCallSaveOnce()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(100, 10, 5, 3);

            Assert.AreEqual(1, fakeRepo.SaveCallCount);
        }

        [Test]
        public void ApplyRunResult_EqualScore_ShouldNotUpdateHighScore()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 100, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(100, 0, 0, 0);

            Assert.AreEqual(100, fakeRepo.StoredData.HighScore);
        }

        [Test]
        public void ApplyRunResult_EqualCombo_ShouldNotUpdateBestCombo()
        {
            fakeRepo.StoredData = new SaveData { BestCombo = 20, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 20, 0, 0);

            Assert.AreEqual(20, fakeRepo.StoredData.BestCombo);
        }

        [Test]
        public void ApplyRunResult_AllZeros_ShouldNotChangeExistingData()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 500, BestCombo = 30, TotalKills = 100, TotalAbsorptions = 50, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, 0, 0);

            Assert.AreEqual(500, fakeRepo.StoredData.HighScore);
            Assert.AreEqual(30, fakeRepo.StoredData.BestCombo);
            Assert.AreEqual(100, fakeRepo.StoredData.TotalKills);
            Assert.AreEqual(50, fakeRepo.StoredData.TotalAbsorptions);
        }

        [Test]
        public void ApplyRunResult_CalledMultipleTimes_ShouldAccumulateKillsAcrossRuns()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, 10, 0);
            service.ApplyRunResult(0, 0, 20, 0);
            service.ApplyRunResult(0, 0, 5, 0);

            Assert.AreEqual(35, fakeRepo.StoredData.TotalKills);
        }

        [Test]
        public void ApplyRunResult_CalledMultipleTimes_ShouldAccumulateAbsorptionsAcrossRuns()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, 0, 15);
            service.ApplyRunResult(0, 0, 0, 25);

            Assert.AreEqual(40, fakeRepo.StoredData.TotalAbsorptions);
        }

        [Test]
        public void ApplyRunResult_CalledMultipleTimes_ShouldKeepHighestScore()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(300, 0, 0, 0);
            service.ApplyRunResult(100, 0, 0, 0);
            service.ApplyRunResult(500, 0, 0, 0);

            Assert.AreEqual(500, fakeRepo.StoredData.HighScore);
        }

        [Test]
        public void ApplyRunResult_CalledMultipleTimes_ShouldKeepHighestCombo()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 15, 0, 0);
            service.ApplyRunResult(0, 5, 0, 0);
            service.ApplyRunResult(0, 25, 0, 0);

            Assert.AreEqual(25, fakeRepo.StoredData.BestCombo);
        }

        [Test]
        public void ApplyRunResult_AfterApply_GetCurrentDataShouldReflectChanges()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(999, 42, 10, 5);

            var data = service.GetCurrentData();
            Assert.AreEqual(999, data.HighScore);
            Assert.AreEqual(42, data.BestCombo);
            Assert.AreEqual(10, data.TotalKills);
            Assert.AreEqual(5, data.TotalAbsorptions);
        }

        [Test]
        public void ApplyRunResult_NegativeKillCount_ShouldNotDecreaseKills()
        {
            fakeRepo.StoredData = new SaveData { TotalKills = 50, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, -10, 0);

            Assert.AreEqual(50, fakeRepo.StoredData.TotalKills);
        }

        [Test]
        public void ApplyRunResult_NegativeAbsorptionCount_ShouldNotDecreaseAbsorptions()
        {
            fakeRepo.StoredData = new SaveData { TotalAbsorptions = 30, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, 0, 0, -5);

            Assert.AreEqual(30, fakeRepo.StoredData.TotalAbsorptions);
        }

        [Test]
        public void ApplyRunResult_NegativeScore_ShouldNotUpdateHighScore()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 100, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(-100, 0, 0, 0);

            Assert.AreEqual(100, fakeRepo.StoredData.HighScore);
        }

        [Test]
        public void ApplyRunResult_NegativeCombo_ShouldNotUpdateBestCombo()
        {
            fakeRepo.StoredData = new SaveData { BestCombo = 20, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.ApplyRunResult(0, -5, 0, 0);

            Assert.AreEqual(20, fakeRepo.StoredData.BestCombo);
        }

        #endregion

        #region Tutorial

        [Test]
        public void MarkTutorialCompleted_ShouldSetFlag()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.MarkTutorialCompleted();

            Assert.IsTrue(fakeRepo.StoredData.TutorialCompleted);
        }

        [Test]
        public void IsTutorialCompleted_WhenNotCompleted_ShouldReturnFalse()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            Assert.IsFalse(service.IsTutorialCompleted());
        }

        [Test]
        public void IsTutorialCompleted_AfterMarkCompleted_ShouldReturnTrue()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.MarkTutorialCompleted();

            Assert.IsTrue(service.IsTutorialCompleted());
        }

        [Test]
        public void MarkTutorialCompleted_CalledTwice_ShouldNotThrow()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.MarkTutorialCompleted();
            service.MarkTutorialCompleted();

            Assert.IsTrue(fakeRepo.StoredData.TutorialCompleted);
            Assert.AreEqual(2, fakeRepo.SaveCallCount);
        }

        [Test]
        public void MarkTutorialCompleted_ShouldCallSaveOnce()
        {
            fakeRepo.StoredData = new SaveData { Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.MarkTutorialCompleted();

            Assert.AreEqual(1, fakeRepo.SaveCallCount);
        }

        [Test]
        public void MarkTutorialCompleted_ShouldNotAffectOtherFields()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 500, BestCombo = 30, TotalKills = 100, TotalAbsorptions = 50, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            service.MarkTutorialCompleted();

            Assert.AreEqual(500, fakeRepo.StoredData.HighScore);
            Assert.AreEqual(30, fakeRepo.StoredData.BestCombo);
            Assert.AreEqual(100, fakeRepo.StoredData.TotalKills);
            Assert.AreEqual(50, fakeRepo.StoredData.TotalAbsorptions);
        }

        #endregion

        #region GetCurrentData

        [Test]
        public void GetCurrentData_ShouldReturnLatestState()
        {
            fakeRepo.StoredData = new SaveData { HighScore = 500, Version = 1 };
            var service = new SaveDataService(fakeRepo);

            var data = service.GetCurrentData();

            Assert.AreEqual(500, data.HighScore);
        }

        #endregion

        private class FakeSaveDataRepository : ISaveDataRepository
        {
            public SaveData StoredData;
            public int SaveCallCount;

            public SaveData Load() => StoredData;

            public void Save(SaveData data)
            {
                StoredData = data;
                SaveCallCount++;
            }
        }
    }
}
