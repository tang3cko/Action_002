using UnityEngine;
using Action002.Audio.Data;
using Action002.Audio.Logic;
using Tang3cko.ReactiveSO;

namespace Action002.Audio.Systems
{
    public class RhythmClockSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RhythmClockConfigSO config;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;

        private double _startDspTime;
        private float _secondsPerHalfBeat;
        private int _currentHalfBeatIndex;
        private int _previousHalfBeatIndex = -1;
        private bool _isPlaying;

        public int CurrentHalfBeatIndex => _currentHalfBeatIndex;
        public bool IsPlaying => _isPlaying;
        public float SecondsPerHalfBeat => _secondsPerHalfBeat;

        public void StartClock()
        {
            if (config == null) return;
            _secondsPerHalfBeat = BeatClockCalculator.SecondsPerHalfBeat(config.Bpm);
            if (_secondsPerHalfBeat <= 0f)
            {
                Debug.LogError($"[{GetType().Name}] Invalid BPM config. Clock not started.", this);
                return;
            }
            _startDspTime = AudioSettings.dspTime + config.StartOffset;
            _previousHalfBeatIndex = -1;
            _currentHalfBeatIndex = 0;
            _isPlaying = true;

            if (bgmSource != null)
                bgmSource.PlayScheduled(_startDspTime);
        }

        public void StopClock()
        {
            _isPlaying = false;
            if (bgmSource != null)
                bgmSource.Stop();
        }

        // Called by GameLoopManager in LateUpdate, before attack systems
        public void ProcessClock()
        {
            if (!_isPlaying || config == null) return;

            double songTime = AudioSettings.dspTime - _startDspTime;
            if (songTime < 0) return; // not started yet

            _previousHalfBeatIndex = _currentHalfBeatIndex;
            _currentHalfBeatIndex = BeatClockCalculator.GetHalfBeatIndex(songTime, _secondsPerHalfBeat);
        }

        // For attack systems to check if they should fire
        public bool ShouldFireOnDownbeat(ref int lastConsumedIndex)
        {
            if (!_isPlaying) return false;
            bool result = BeatGateCalculator.ShouldFireOnDownbeat(_currentHalfBeatIndex, lastConsumedIndex);
            if (result) lastConsumedIndex = _currentHalfBeatIndex;
            return result;
        }

        public bool ShouldFireOnOffbeat(ref int lastConsumedIndex)
        {
            if (!_isPlaying) return false;
            bool result = BeatGateCalculator.ShouldFireOnOffbeat(_currentHalfBeatIndex, lastConsumedIndex);
            if (result) lastConsumedIndex = _currentHalfBeatIndex;
            return result;
        }

        public void ResetForNewRun()
        {
            StopClock();
            _previousHalfBeatIndex = -1;
            _currentHalfBeatIndex = 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config == null) Debug.LogWarning($"[{GetType().Name}] config not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
