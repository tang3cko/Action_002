namespace Action002.Audio.Data
{
    public readonly struct BeatTickInfo
    {
        public readonly int HalfBeatIndex;
        public readonly bool IsDownbeat;
        public readonly float Phase;

        public BeatTickInfo(int halfBeatIndex, bool isDownbeat, float phase)
        {
            HalfBeatIndex = halfBeatIndex;
            IsDownbeat = isDownbeat;
            Phase = phase;
        }
    }
}
