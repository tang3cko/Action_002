namespace Action002.Audio.Logic
{
    public static class BeatClockCalculator
    {
        public static int GetHalfBeatIndex(double songTime, float secondsPerHalfBeat)
        {
            if (secondsPerHalfBeat <= 0f) return 0;
            return (int)(songTime / secondsPerHalfBeat);
        }

        public static bool IsDownbeat(int halfBeatIndex)
        {
            return halfBeatIndex % 2 == 0;
        }

        public static float SecondsPerHalfBeat(float bpm)
        {
            if (bpm <= 0f) return 0.25f;
            return 30f / bpm;
        }
    }
}
