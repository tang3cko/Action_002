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
        private bool isPlaying;

        public int CurrentHalfBeatIndex => currentHalfBeatIndex;
        public bool IsPlaying => isPlaying;
        public float SecondsPerHalfBeat => secondsPerHalfBeat;

        public RhythmClock(RhythmClockConfigSO config, Func<double> dspTimeSource)
        {
            this.config = config;
            this.dspTimeSource = dspTimeSource;
        }

        public bool StartClock()
        {
            if (config == null)
            {
                UnityEngine.Debug.LogError("[RhythmClock] config is null. Clock not started.");
                return false;
            }
            if (config.Bpm <= 0f)
            {
                UnityEngine.Debug.LogError("[RhythmClock] Invalid BPM config. Clock not started.");
                return false;
            }
            secondsPerHalfBeat = BeatClockCalculator.SecondsPerHalfBeat(config.Bpm);
            if (secondsPerHalfBeat <= 0f)
            {
                UnityEngine.Debug.LogError("[RhythmClock] SecondsPerHalfBeat is invalid. Clock not started.");
                return false;
            }
            startDspTime = dspTimeSource() + config.StartOffset;
            currentHalfBeatIndex = 0;
            isPlaying = true;
            return true;
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
            currentHalfBeatIndex = 0;
        }
    }
}
