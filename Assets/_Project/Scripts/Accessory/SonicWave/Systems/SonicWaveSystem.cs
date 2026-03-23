using UnityEngine;
using Action002.Accessory.SonicWave.Data;
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

        public void ProcessAttacks()
        {
            if (sonicWave == null) return;
            sonicWave.ProcessAttacks();
        }

        public void ResetForNewRun()
        {
            sonicWave?.ResetForNewRun();
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
