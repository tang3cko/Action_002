namespace Action002.Core.Save
{
    public class SaveDataService
    {
        private readonly ISaveDataRepository repository;
        private SaveData cached;

        public SaveDataService(ISaveDataRepository repository)
        {
            this.repository = repository;
            cached = repository.Load();
        }

        public void ApplyRunResult(int finalScore, int maxCombo, int killCount, int absorptionCount)
        {
            if (finalScore > 0 && finalScore > cached.HighScore)
                cached.HighScore = finalScore;

            if (maxCombo > 0 && maxCombo > cached.BestCombo)
                cached.BestCombo = maxCombo;

            if (killCount > 0)
                cached.TotalKills += killCount;

            if (absorptionCount > 0)
                cached.TotalAbsorptions += absorptionCount;

            repository.Save(cached);
        }

        public void MarkTutorialCompleted()
        {
            cached.TutorialCompleted = true;
            repository.Save(cached);
        }

        public bool IsTutorialCompleted()
        {
            return cached.TutorialCompleted;
        }

        public SaveData GetCurrentData()
        {
            return cached;
        }
    }
}
