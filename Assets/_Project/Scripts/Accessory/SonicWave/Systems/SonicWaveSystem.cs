using UnityEngine;
using UnityEngine.Serialization;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Logic;
using Action002.Audio.Systems;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Accessory.SonicWave.Systems
{
    public class SonicWaveSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;

        [Header("Sets")]
        [SerializeField] private WaveStateSetSO waveSet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [FormerlySerializedAs("rightWaveClip")]
        [SerializeField] private AudioClip smallPulse1Clip;
        [FormerlySerializedAs("leftWaveClip")]
        [SerializeField] private AudioClip smallPulse2Clip;
        [FormerlySerializedAs("pulseClip")]
        [SerializeField] private AudioClip largePulseClip;

        private SonicWave sonicWave;

        /// <summary>IAccessory として AccessoryManager に登録するための参照。</summary>
        public IAccessory Accessory => sonicWave;

        private void Awake()
        {
            sonicWave = new SonicWave(
                rhythmClock, waveSet,
                playerPositionVar, playerPolarityVar,
                gameConfig != null ? gameConfig.WaveBaseMaxRadius : 5f,
                gameConfig != null ? gameConfig.WaveBaseExpandSpeed : 8f);
        }

        /// <summary>
        /// 毎フレーム GameLoopManager から呼ばれる。
        /// SonicWave の攻撃処理を実行し、SE を再生する。
        /// Level は AccessoryManager が直接管理するため、SO変数の読み取りは不要。
        /// </summary>
        public void ProcessAttacks()
        {
            if (sonicWave == null) return;

            sonicWave.ProcessAttacks();

            if (sonicWave.LastFiredBeat.HasValue)
                PlaySfx(sonicWave.LastFiredBeat.Value, sonicWave.LastFiredBeatInPattern);
        }

        public void ResetForNewRun()
        {
            sonicWave?.ResetForNewRun();
        }

        private void PlaySfx(SonicWaveBeat beat, int beatInPattern)
        {
            if (sfxSource == null) return;
            AudioClip clip;
            switch (beat)
            {
                case SonicWaveBeat.SmallPulse:
                    // パターン内 0 → smallPulse1Clip, 1 → smallPulse2Clip
                    clip = beatInPattern == 0 ? smallPulse1Clip : smallPulse2Clip;
                    break;
                case SonicWaveBeat.LargePulse:
                    clip = largePulseClip;
                    break;
                default:
                    clip = null;
                    break;
            }
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (waveSet == null) Debug.LogWarning($"[{GetType().Name}] waveSet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
