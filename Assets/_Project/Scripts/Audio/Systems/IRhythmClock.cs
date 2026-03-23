namespace Action002.Audio.Systems
{
    public interface IRhythmClock
    {
        int CurrentHalfBeatIndex { get; }
        bool StartClock();
        void StopClock();
        void ProcessClock();
        bool ShouldFireOnDownbeat(ref int lastConsumedIndex);
        bool ShouldFireOnOffbeat(ref int lastConsumedIndex);
        void ResetForNewRun();
    }
}
