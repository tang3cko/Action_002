namespace Action002.Core.Flow
{
    public enum GameResultType : byte
    {
        Clear = 0,
        GameOver = 1
    }

    public readonly struct GameResultContext
    {
        public GameResultType ResultType { get; }
        public int FinalScore { get; }

        public GameResultContext(GameResultType resultType, int finalScore)
        {
            ResultType = resultType;
            FinalScore = finalScore;
        }
    }
}
