using UnityEngine;
using Action002.Audio.Data;
using Action002.Audio.Logic;
using Tang3cko.ReactiveSO;

namespace Action002.Audio.Systems
{
    public class RhythmClockSystem : MonoBehaviour, IRhythmClock
    {
        [Header("Config")]
        [SerializeField] private RhythmClockConfigSO config;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;

        private double startDspTime;
        private float secondsPerHalfBeat;
        private int currentHalfBeatIndex;
        private int previousHalfBeatIndex = -1;
        private bool isPlaying;

        public int CurrentHalfBeatIndex => currentHalfBeatIndex;
        public bool IsPlaying => isPlaying;
        public float SecondsPerHalfBeat => secondsPerHalfBeat;

        public void StartClock()
        {
            if (config == null) return;
            secondsPerHalfBeat = BeatClockCalculator.SecondsPerHalfBeat(config.Bpm);
            if (secondsPerHalfBeat <= 0f)
            {
                Debug.LogError($"[{GetType().Name}] Invalid BPM config. Clock not started.", this);
                return;
            }
            startDspTime = AudioSettings.dspTime + config.StartOffset;
            previousHalfBeatIndex = -1;
            currentHalfBeatIndex = 0;
            isPlaying = true;

            if (bgmSource != null)
                bgmSource.PlayScheduled(startDspTime);
        }

        public void StopClock()
        {
            isPlaying = false;
            if (bgmSource != null)
                bgmSource.Stop();
        }

        // Called by GameLoopManager in LateUpdate, before attack systems
        public void ProcessClock()
        {
            if (!isPlaying || config == null) return;

            double songTime = AudioSettings.dspTime - startDspTime;
            if (songTime < 0) return; // not started yet

            previousHalfBeatIndex = currentHalfBeatIndex;
            currentHalfBeatIndex = BeatClockCalculator.GetHalfBeatIndex(songTime, secondsPerHalfBeat);
        }

        // For attack systems to check if they should fire
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config == null) Debug.LogWarning($"[{GetType().Name}] config not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
