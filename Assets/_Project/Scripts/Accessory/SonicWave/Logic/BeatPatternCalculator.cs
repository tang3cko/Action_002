namespace Action002.Accessory.SonicWave.Logic
{
    public enum SonicWaveBeat : byte
    {
        SmallPulse = 0,
        LargePulse = 1,
        Rest       = 2,
    }

    public static class BeatPatternCalculator
    {
        public const int PATTERN_LENGTH = 8;

        /// <summary>
        /// currentHalfBeatIndex から8 halfbeat (= 4拍 = 2秒) サイクル内の
        /// ビート種別を返す。裏拍も含めた直接マッピング。
        /// </summary>
        public static SonicWaveBeat GetCurrentBeat(int currentHalfBeatIndex)
        {
            int beatInPattern = currentHalfBeatIndex % PATTERN_LENGTH;
            switch (beatInPattern)
            {
                case 0: return SonicWaveBeat.SmallPulse;
                case 1: return SonicWaveBeat.SmallPulse;
                case 2: return SonicWaveBeat.LargePulse;
                default: return SonicWaveBeat.Rest;
            }
        }

        public static bool IsRestBeat(SonicWaveBeat beat)
        {
            return beat == SonicWaveBeat.Rest;
        }
    }
}
