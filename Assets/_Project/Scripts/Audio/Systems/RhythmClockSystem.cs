using UnityEngine;
using Action002.Audio.Data;

namespace Action002.Audio.Systems
{
    public class RhythmClockSystem : MonoBehaviour, IRhythmClock
    {
        [Header("Config")]
        [SerializeField] private RhythmClockConfigSO config;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.7f;

        private RhythmClock clock;

        public int CurrentHalfBeatIndex => clock?.CurrentHalfBeatIndex ?? 0;
        public bool IsPlaying => clock?.IsPlaying ?? false;
        public float SecondsPerHalfBeat => clock?.SecondsPerHalfBeat ?? 0f;

        private void Awake()
        {
            clock = new RhythmClock(config, () => AudioSettings.dspTime);
        }

        public bool StartClock()
        {
            if (clock == null)
                clock = new RhythmClock(config, () => AudioSettings.dspTime);

            bool success = clock.StartClock();
            if (success && bgmSource != null)
            {
                bgmSource.volume = bgmVolume;
                double scheduledTime = AudioSettings.dspTime + (config != null ? config.StartOffset : 0.0);
                bgmSource.PlayScheduled(scheduledTime);
            }
            return success;
        }

        public void StopClock()
        {
            clock?.StopClock();
            if (bgmSource != null)
                bgmSource.Stop();
        }

        public void ProcessClock()
        {
            clock?.ProcessClock();
        }

        public bool ShouldFireOnDownbeat(ref int lastConsumedIndex)
        {
            if (clock == null) return false;
            return clock.ShouldFireOnDownbeat(ref lastConsumedIndex);
        }

        public bool ShouldFireOnOffbeat(ref int lastConsumedIndex)
        {
            if (clock == null) return false;
            return clock.ShouldFireOnOffbeat(ref lastConsumedIndex);
        }

        public void ResetForNewRun()
        {
            clock?.ResetForNewRun();
            if (bgmSource != null)
                bgmSource.Stop();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config == null) Debug.LogWarning($"[{GetType().Name}] config not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
