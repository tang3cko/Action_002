namespace Action002.Audio.Systems
{
    public interface IRhythmClock
    {
        bool IsPlaying { get; }
        int CurrentHalfBeatIndex { get; }
        float SecondsPerHalfBeat { get; }
        bool StartClock();
        void StopClock();
        void ProcessClock();
        bool ShouldFireOnDownbeat(ref int lastConsumedIndex);
        bool ShouldFireOnOffbeat(ref int lastConsumedIndex);
        void ResetForNewRun();
    }
}
