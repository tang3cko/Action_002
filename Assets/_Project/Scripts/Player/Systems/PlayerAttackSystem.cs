using UnityEngine;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Player.Systems
{
    public class PlayerAttackSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;

        private PlayerAttack attack;

        private void Awake()
        {
            attack = new PlayerAttack(
                rhythmClock,
                gameConfig,
                enemySet,
                bulletSet,
                playerPositionVar,
                playerPolarityVar);
        }

        public void ProcessAttacks()
        {
            attack.ProcessAttacks();
        }

        public void ResetForNewRun()
        {
            attack.ResetForNewRun();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
