using UnityEngine;
using LitMotion;
using Action002.Audio.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Audio.Systems
{
    public class RhythmClockSystem : MonoBehaviour, IRhythmClock
    {
        [Header("Config")]
        [SerializeField] private RhythmClockConfigSO config;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.7f;

        [Header("Layer: Arpeggio (wave unlock)")]
        [SerializeField] private AudioSource arpeggioSource;
        [SerializeField, Range(0f, 1f)] private float arpeggioVolume = 0.5f;
        [SerializeField] private int arpeggioUnlockLevel = 3;
        [SerializeField] private float arpeggioFadeDuration = 2f;

        [Header("Layer: Pad (90s)")]
        [SerializeField] private AudioSource padSource;
        [SerializeField, Range(0f, 1f)] private float padVolume = 0.4f;
        [SerializeField] private float padUnlockTime = 90f;
        [SerializeField] private float padFadeDuration = 4f;

        [Header("Layer: Melody (120s, 3 tracks)")]
        [SerializeField] private AudioSource[] melodySources = new AudioSource[3];
        [SerializeField, Range(0f, 1f)] private float melodyVolume = 0.5f;
        [SerializeField] private float melodyUnlockTime = 120f;
        [SerializeField] private float melodyFadeDuration = 4f;

        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onPlayerLevelUp;

        private RhythmClock clock;
        private bool arpeggioUnlocked;
        private bool padUnlocked;
        private bool melodyUnlocked;
        private float elapsedTime;
        private MotionHandle arpeggioFadeHandle;
        private MotionHandle padFadeHandle;
        private MotionHandle[] melodyFadeHandles = new MotionHandle[3];

        public int CurrentHalfBeatIndex => clock?.CurrentHalfBeatIndex ?? 0;
        public bool IsPlaying => clock?.IsPlaying ?? false;
        public float SecondsPerHalfBeat => clock?.SecondsPerHalfBeat ?? 0f;

        private void Awake()
        {
            clock = new RhythmClock(config, () => AudioSettings.dspTime);
        }

        private void OnEnable()
        {
            if (onPlayerLevelUp != null)
                onPlayerLevelUp.OnEventRaised += HandlePlayerLevelUp;
        }

        private void OnDisable()
        {
            if (onPlayerLevelUp != null)
                onPlayerLevelUp.OnEventRaised -= HandlePlayerLevelUp;
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

                StartLayerSilent(arpeggioSource, scheduledTime);
                StartLayerSilent(padSource, scheduledTime);
                for (int i = 0; i < melodySources.Length; i++)
                    StartLayerSilent(melodySources[i], scheduledTime);
            }
            return success;
        }

        public void StopClock()
        {
            clock?.StopClock();
            CancelFades();
            StopSource(bgmSource);
            StopSource(arpeggioSource);
            StopSource(padSource);
            for (int i = 0; i < melodySources.Length; i++)
                StopSource(melodySources[i]);
        }

        public void ProcessClock()
        {
            clock?.ProcessClock();

            if (IsPlaying && (!padUnlocked || !melodyUnlocked))
            {
                elapsedTime += Time.deltaTime;
                if (!padUnlocked && elapsedTime >= padUnlockTime)
                    UnlockPad();
                if (!melodyUnlocked && elapsedTime >= melodyUnlockTime)
                    UnlockMelody();
            }
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
            CancelFades();
            StopSource(bgmSource);
            StopSource(arpeggioSource);
            StopSource(padSource);
            for (int i = 0; i < melodySources.Length; i++)
                StopSource(melodySources[i]);
            arpeggioUnlocked = false;
            padUnlocked = false;
            melodyUnlocked = false;
            elapsedTime = 0f;
        }

        private void HandlePlayerLevelUp(int level)
        {
            if (arpeggioUnlocked) return;
            if (level < arpeggioUnlockLevel) return;
            if (arpeggioSource == null) return;

            arpeggioUnlocked = true;
            arpeggioFadeHandle = LMotion.Create(0f, arpeggioVolume, arpeggioFadeDuration)
                .WithEase(Ease.InCubic)
                .Bind(v => arpeggioSource.volume = v);
        }

        private void UnlockPad()
        {
            if (padSource == null) return;

            padUnlocked = true;
            padFadeHandle = LMotion.Create(0f, padVolume, padFadeDuration)
                .WithEase(Ease.InCubic)
                .Bind(v => padSource.volume = v);
        }

        private void UnlockMelody()
        {
            melodyUnlocked = true;
            for (int i = 0; i < melodySources.Length; i++)
            {
                if (melodySources[i] == null) continue;
                int idx = i;
                melodyFadeHandles[i] = LMotion.Create(0f, melodyVolume, melodyFadeDuration)
                    .WithEase(Ease.InCubic)
                    .Bind(v => melodySources[idx].volume = v);
            }
        }

        private static void StartLayerSilent(AudioSource source, double scheduledTime)
        {
            if (source != null && source.clip != null)
            {
                source.volume = 0f;
                source.PlayScheduled(scheduledTime);
            }
        }

        private static void StopSource(AudioSource source)
        {
            if (source != null)
                source.Stop();
        }

        private void CancelFades()
        {
            if (arpeggioFadeHandle.IsActive())
                arpeggioFadeHandle.Cancel();
            if (padFadeHandle.IsActive())
                padFadeHandle.Cancel();
            for (int i = 0; i < melodyFadeHandles.Length; i++)
            {
                if (melodyFadeHandles[i].IsActive())
                    melodyFadeHandles[i].Cancel();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config == null) Debug.LogWarning($"[{GetType().Name}] config not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
