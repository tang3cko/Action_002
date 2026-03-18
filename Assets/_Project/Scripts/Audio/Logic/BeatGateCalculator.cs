namespace Action002.Audio.Logic
{
    public static class BeatGateCalculator
    {
        public static bool ShouldFire(int currentHalfBeatIndex, int lastConsumedIndex)
        {
            return currentHalfBeatIndex > lastConsumedIndex;
        }

        public static bool ShouldFireOnDownbeat(int currentHalfBeatIndex, int lastConsumedIndex)
        {
            return ShouldFire(currentHalfBeatIndex, lastConsumedIndex)
                && BeatClockCalculator.IsDownbeat(currentHalfBeatIndex);
        }

        public static bool ShouldFireOnOffbeat(int currentHalfBeatIndex, int lastConsumedIndex)
        {
            return ShouldFire(currentHalfBeatIndex, lastConsumedIndex)
                && !BeatClockCalculator.IsDownbeat(currentHalfBeatIndex);
        }
    }
}
