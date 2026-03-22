using System;

namespace Action002.Core.Events
{
    [Serializable]
    public struct ScoreSubmitRequest
    {
        public int Score;
        public int Combo;

        public ScoreSubmitRequest(int score, int combo)
        {
            Score = score;
            Combo = combo;
        }
    }
}
