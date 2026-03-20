namespace Action002.Audio.Systems
{
    public interface IRhythmClock
    {
        bool IsPlaying { get; }
        int CurrentHalfBeatIndex { get; }
        float SecondsPerHalfBeat { get; }
        void StartClock(double currentDspTime);
        void StopClock();
        void ProcessClock(double currentDspTime);
        bool ShouldFireOnDownbeat(ref int lastConsumedIndex);
        bool ShouldFireOnOffbeat(ref int lastConsumedIndex);
        void ResetForNewRun();
    }
}
