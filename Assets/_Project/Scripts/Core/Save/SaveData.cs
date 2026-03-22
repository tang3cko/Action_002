using System;

namespace Action002.Core.Save
{
    [Serializable]
    public struct SaveData
    {
        public int HighScore;
        public bool TutorialCompleted;
        public int BestCombo;
        public int TotalKills;
        public int TotalAbsorptions;
        public int Version;
    }
}
