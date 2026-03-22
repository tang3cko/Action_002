using UnityEngine;
using Action002.Bullet.Data;
using Action002.Core;
using Action002.Enemy.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Bullet.Systems
{
    public class BulletCollisionSystem : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private BulletStateSetSO bulletSet;
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;

        [Header("Events (subscribe)")]
        [SerializeField] private GameObjectEventChannelSO onBossHitTargetChanged;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;
        [SerializeField] private IntEventChannelSO onEnemyKilled;
        [SerializeField] private FloatEventChannelSO onComboIncremented;
        [SerializeField] private IntEventChannelSO onKillScoreAdded;

        [Header("Dependencies")]
        [SerializeField] private EnemyDeathBufferSO deathBuffer;

        [Header("Settings")]
        [SerializeField] private float absorbRadius = 1.0f;
        [SerializeField] private float damageRadius = 0.3f;
        [SerializeField] private float bulletHitRadius = 0.5f;
        [SerializeField] private int killScore = 50;

        private BulletCollision logic;

        private void Awake()
        {
            logic = new BulletCollision(
                bulletSet,
                enemySet,
                playerPositionVar,
                playerPolarityVar,
                onPlayerDamaged,
                onEnemyKilled,
                onComboIncremented,
                onKillScoreAdded,
                deathBuffer,
                absorbRadius,
                damageRadius,
                bulletHitRadius,
                killScore
            );
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

        public void SetBossHitTarget(IBossHitTarget target)
        {
            if (logic == null) return;
            logic.SetBossHitTarget(target);
        }

        public void ProcessCollisions()
        {
            if (logic == null) return;
            logic.ProcessCollisions();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
            if (onPlayerDamaged == null) Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
            if (onEnemyKilled == null) Debug.LogWarning($"[{GetType().Name}] onEnemyKilled not assigned on {gameObject.name}.", this);
            if (onComboIncremented == null) Debug.LogWarning($"[{GetType().Name}] onComboIncremented not assigned on {gameObject.name}.", this);
            if (onKillScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onKillScoreAdded not assigned on {gameObject.name}.", this);
            if (onBossHitTargetChanged == null) Debug.LogWarning($"[{GetType().Name}] onBossHitTargetChanged not assigned on {gameObject.name}.", this);
            if (deathBuffer == null) Debug.LogWarning($"[{GetType().Name}] deathBuffer not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
