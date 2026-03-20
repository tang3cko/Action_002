using System;
using Action002.Audio.Data;
using Action002.Audio.Logic;

namespace Action002.Audio.Systems
{
    public class RhythmClock : IRhythmClock
    {
        private readonly RhythmClockConfigSO config;
        private readonly Func<double> dspTimeSource;

        private double startDspTime;
        private float secondsPerHalfBeat;
        private int currentHalfBeatIndex;
        private int previousHalfBeatIndex = -1;
        private bool isPlaying;

        public int CurrentHalfBeatIndex => currentHalfBeatIndex;
        public bool IsPlaying => isPlaying;
        public float SecondsPerHalfBeat => secondsPerHalfBeat;

        public RhythmClock(RhythmClockConfigSO config, Func<double> dspTimeSource)
        {
            this.config = config;
            this.dspTimeSource = dspTimeSource;
        }

        public void StartClock()
        {
            if (config == null) return;
            if (config.Bpm <= 0f) return;
            secondsPerHalfBeat = BeatClockCalculator.SecondsPerHalfBeat(config.Bpm);
            if (secondsPerHalfBeat <= 0f) return;
            startDspTime = dspTimeSource() + config.StartOffset;
            previousHalfBeatIndex = -1;
            currentHalfBeatIndex = 0;
            isPlaying = true;
        }

        public void StopClock()
        {
            isPlaying = false;
        }

        public void ProcessClock()
        {
            if (!isPlaying || config == null) return;

            double songTime = dspTimeSource() - startDspTime;
            if (songTime < 0) return;

            previousHalfBeatIndex = currentHalfBeatIndex;
            currentHalfBeatIndex = BeatClockCalculator.GetHalfBeatIndex(songTime, secondsPerHalfBeat);
        }

        public bool ShouldFireOnDownbeat(ref int lastConsumedIndex)
        {
            if (!isPlaying) return false;
            bool result = BeatGateCalculator.ShouldFireOnDownbeat(currentHalfBeatIndex, lastConsumedIndex);
            if (result) lastConsumedIndex = currentHalfBeatIndex;
            return result;
        }

        public bool ShouldFireOnOffbeat(ref int lastConsumedIndex)
        {
            if (!isPlaying) return false;
            bool result = BeatGateCalculator.ShouldFireOnOffbeat(currentHalfBeatIndex, lastConsumedIndex);
            if (result) lastConsumedIndex = currentHalfBeatIndex;
            return result;
        }

        public void ResetForNewRun()
        {
            StopClock();
            previousHalfBeatIndex = -1;
            currentHalfBeatIndex = 0;
        }
    }
}
