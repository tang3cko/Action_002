namespace Action002.Core.Save
{
    public class RunSessionStatsCalculator
    {
        public int MaxCombo { get; private set; }
        public int KillCount { get; private set; }
        public int AbsorptionCount { get; private set; }

        public void Reset()
        {
            MaxCombo = 0;
            KillCount = 0;
            AbsorptionCount = 0;
        }

        public void RecordKill()
        {
            KillCount++;
        }

        public void RecordAbsorption(int currentCombo)
        {
            AbsorptionCount++;
            if (currentCombo > MaxCombo)
                MaxCombo = currentCombo;
        }
    }
}
