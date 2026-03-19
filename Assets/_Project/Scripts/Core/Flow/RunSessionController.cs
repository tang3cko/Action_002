using UnityEngine;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Bullet.Systems;
using Action002.Enemy.Data;
using Action002.Enemy.Systems;
using Action002.Input;
using Action002.Player.Systems;

namespace Action002.Core.Flow
{
    public class RunSessionController : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerAttackSystem playerAttackSystem;

        [Header("Enemy")]
        [SerializeField] private EnemySpawnSystem enemySpawnSystem;

        [Header("Bullet")]
        [SerializeField] private EnemyShootSystem enemyShootSystem;

        [Header("Audio")]
        [SerializeField] private RhythmClockSystem rhythmClockSystem;

        [Header("Core")]
        [SerializeField] private GameLoopManager gameLoopManager;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemyStateSet;
        [SerializeField] private BulletStateSetSO bulletStateSet;

        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        public void SetPhase(GamePhase phase)
        {
            bool active = phase == GamePhase.Stage || phase == GamePhase.Boss;

            if (gameLoopManager != null)
                gameLoopManager.SetRunning(active);

            if (inputReader != null)
            {
                if (active)
                    inputReader.EnablePlayerInput();
                else
                    inputReader.DisablePlayerInput();
            }
        }

        public void StartRun()
        {
            if (enemySpawnSystem != null)
                enemySpawnSystem.enabled = true;
            if (inputReader != null)
                inputReader.EnablePlayerInput();
            if (rhythmClockSystem != null)
                rhythmClockSystem.StartClock();
            if (gameLoopManager != null)
                gameLoopManager.SetRunning(true);
        }

        public void StopRunLoop()
        {
            if (inputReader != null)
                inputReader.DisablePlayerInput();
            if (rhythmClockSystem != null)
                rhythmClockSystem.StopClock();
            if (gameLoopManager != null)
                gameLoopManager.SetRunning(false);
        }

        public void ResetRun()
        {
            if (enemyStateSet != null)
                enemyStateSet.Clear();
            if (bulletStateSet != null)
                bulletStateSet.Clear();

            if (playerController != null)
                playerController.ResetForNewRun();
            if (playerAttackSystem != null)
                playerAttackSystem.ResetForNewRun();
            if (enemySpawnSystem != null)
                enemySpawnSystem.ResetForNewRun();
            if (enemyShootSystem != null)
                enemyShootSystem.ResetForNewRun();
            if (rhythmClockSystem != null)
                rhythmClockSystem.ResetForNewRun();
        }

        public void CleanupRunState()
        {
            StopRunLoop();
            ResetRun();
        }

        public void PrepareBossPhase()
        {
            if (enemySpawnSystem != null)
                enemySpawnSystem.enabled = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (playerController == null) Debug.LogWarning($"[{GetType().Name}] playerController not assigned on {gameObject.name}.", this);
            if (playerAttackSystem == null) Debug.LogWarning($"[{GetType().Name}] playerAttackSystem not assigned on {gameObject.name}.", this);
            if (enemySpawnSystem == null) Debug.LogWarning($"[{GetType().Name}] enemySpawnSystem not assigned on {gameObject.name}.", this);
            if (enemyShootSystem == null) Debug.LogWarning($"[{GetType().Name}] enemyShootSystem not assigned on {gameObject.name}.", this);
            if (rhythmClockSystem == null) Debug.LogWarning($"[{GetType().Name}] rhythmClockSystem not assigned on {gameObject.name}.", this);
            if (gameLoopManager == null) Debug.LogWarning($"[{GetType().Name}] gameLoopManager not assigned on {gameObject.name}.", this);
            if (enemyStateSet == null) Debug.LogWarning($"[{GetType().Name}] enemyStateSet not assigned on {gameObject.name}.", this);
            if (bulletStateSet == null) Debug.LogWarning($"[{GetType().Name}] bulletStateSet not assigned on {gameObject.name}.", this);
            if (inputReader == null) Debug.LogWarning($"[{GetType().Name}] inputReader not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
