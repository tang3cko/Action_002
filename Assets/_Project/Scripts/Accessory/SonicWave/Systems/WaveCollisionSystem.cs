using UnityEngine;
using Action002.Accessory.SonicWave.Data;
using Action002.Core;
using Action002.Enemy.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Accessory.SonicWave.Systems
{
    public class WaveCollisionSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private WaveStateSetSO waveSet;
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Dependencies")]
        [SerializeField] private EnemyDeathBufferSO deathBuffer;

        [Header("Events (subscribe)")]
        [SerializeField] private GameObjectEventChannelSO onBossHitTargetChanged;

        [Header("Events (publish)")]
        [SerializeField] private IntEventChannelSO onEnemyKilled;
        [SerializeField] private IntEventChannelSO onKillScoreAdded;

        [Header("Settings")]
        [SerializeField] private int killScore = 50;

        private WaveCollision logic;

        private void Awake()
        {
            logic = new WaveCollision(
                waveSet, enemySet, deathBuffer,
                onEnemyKilled, onKillScoreAdded,
                gameConfig, killScore);
        }

        private void OnEnable()
        {
            if (onBossHitTargetChanged != null)
                onBossHitTargetChanged.OnEventRaised += HandleBossHitTargetChanged;
        }

        private void OnDisable()
        {
            if (onBossHitTargetChanged != null)
                onBossHitTargetChanged.OnEventRaised -= HandleBossHitTargetChanged;
        }

        private void HandleBossHitTargetChanged(GameObject go)
        {
            if (logic == null) return;

            if (go != null)
            {
                var target = go.GetComponent<IBossHitTarget>();
                logic.SetBossHitTarget(target);
            }
            else
            {
                logic.SetBossHitTarget(null);
            }
        }

        public void ProcessCollisions()
        {
            if (logic == null) return;
            logic.ProcessCollisions();
        }

        public void ResetForNewRun()
        {
            logic?.ResetForNewRun();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (waveSet == null) Debug.LogWarning($"[{GetType().Name}] waveSet not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (deathBuffer == null) Debug.LogWarning($"[{GetType().Name}] deathBuffer not assigned on {gameObject.name}.", this);
            if (onBossHitTargetChanged == null) Debug.LogWarning($"[{GetType().Name}] onBossHitTargetChanged not assigned on {gameObject.name}.", this);
            if (onEnemyKilled == null) Debug.LogWarning($"[{GetType().Name}] onEnemyKilled not assigned on {gameObject.name}.", this);
            if (onKillScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onKillScoreAdded not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
