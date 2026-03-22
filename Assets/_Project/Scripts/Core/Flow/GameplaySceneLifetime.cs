using UnityEngine;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Action002.Input;
using Tang3cko.ReactiveSO;

namespace Action002.Core.Flow
{
    public class GameplaySceneLifetime : MonoBehaviour, IGameplayStartupActions
    {
        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemyStateSet;
        [SerializeField] private BulletStateSetSO bulletStateSet;

        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("Systems")]
        [SerializeField] private GameLoopManager gameLoopManager;
        [SerializeField] private RhythmClockSystem rhythmClockSystem;

        [Header("Variables (reset)")]
        [SerializeField] private IntVariableSO playerHpVar;
        [SerializeField] private IntVariableSO scoreVar;
        [SerializeField] private IntVariableSO comboCountVar;
        [SerializeField] private FloatVariableSO spinGaugeVar;
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;
        [SerializeField] private IntVariableSO maxComboVar;
        [SerializeField] private IntVariableSO runKillCountVar;
        [SerializeField] private IntVariableSO runAbsorptionCountVar;

        private void Awake()
        {
            if (enemyStateSet != null) enemyStateSet.Clear();
            if (bulletStateSet != null) bulletStateSet.Clear();

            if (playerHpVar != null) playerHpVar.ResetToInitial();
            if (scoreVar != null) scoreVar.ResetToInitial();
            if (comboCountVar != null) comboCountVar.ResetToInitial();
            if (spinGaugeVar != null) spinGaugeVar.ResetToInitial();
            if (playerPositionVar != null) playerPositionVar.ResetToInitial();
            if (playerPolarityVar != null) playerPolarityVar.ResetToInitial();
            if (maxComboVar != null) maxComboVar.ResetToInitial();
            if (runKillCountVar != null) runKillCountVar.ResetToInitial();
            if (runAbsorptionCountVar != null) runAbsorptionCountVar.ResetToInitial();
        }

        private void Start()
        {
            var logic = new GameplayStartupLogic(this);
            logic.Execute();
        }

        // --- IGameplayStartupActions implementation ---

        void IGameplayStartupActions.DisablePlayerInput()
        {
            if (inputReader != null) inputReader.DisablePlayerInput();
        }

        void IGameplayStartupActions.EnablePlayerInput()
        {
            if (inputReader != null) inputReader.EnablePlayerInput();
        }

        void IGameplayStartupActions.ResetForNewRun()
        {
            if (rhythmClockSystem != null) rhythmClockSystem.ResetForNewRun();
        }

        bool IGameplayStartupActions.StartClock()
        {
            if (rhythmClockSystem == null)
            {
                Debug.LogError("[GameplaySceneLifetime] RhythmClockSystem is not assigned.", this);
                return false;
            }
            return rhythmClockSystem.StartClock();
        }

        void IGameplayStartupActions.LogStartupError(string message)
        {
            Debug.LogError(message, this);
        }

        void IGameplayStartupActions.SetRunning(bool running)
        {
            if (gameLoopManager != null) gameLoopManager.SetRunning(running);
        }

        private void OnDestroy()
        {
            // Scene unloading - cleanup
            if (gameLoopManager != null) gameLoopManager.StopAndCleanup();
            if (rhythmClockSystem != null) rhythmClockSystem.StopClock();
            if (inputReader != null) inputReader.DisablePlayerInput();

            // Clear entity sets (SO persists across scenes)
            if (enemyStateSet != null) enemyStateSet.Clear();
            if (bulletStateSet != null) bulletStateSet.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemyStateSet == null) Debug.LogWarning($"[{GetType().Name}] enemyStateSet not assigned on {gameObject.name}.", this);
            if (bulletStateSet == null) Debug.LogWarning($"[{GetType().Name}] bulletStateSet not assigned on {gameObject.name}.", this);
            if (inputReader == null) Debug.LogWarning($"[{GetType().Name}] inputReader not assigned on {gameObject.name}.", this);
            if (gameLoopManager == null) Debug.LogWarning($"[{GetType().Name}] gameLoopManager not assigned on {gameObject.name}.", this);
            if (rhythmClockSystem == null) Debug.LogWarning($"[{GetType().Name}] rhythmClockSystem not assigned on {gameObject.name}.", this);
            if (playerHpVar == null) Debug.LogWarning($"[{GetType().Name}] playerHpVar not assigned on {gameObject.name}.", this);
            if (scoreVar == null) Debug.LogWarning($"[{GetType().Name}] scoreVar not assigned on {gameObject.name}.", this);
            if (comboCountVar == null) Debug.LogWarning($"[{GetType().Name}] comboCountVar not assigned on {gameObject.name}.", this);
            if (spinGaugeVar == null) Debug.LogWarning($"[{GetType().Name}] spinGaugeVar not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
            if (maxComboVar == null) Debug.LogWarning($"[{GetType().Name}] maxComboVar not assigned on {gameObject.name}.", this);
            if (runKillCountVar == null) Debug.LogWarning($"[{GetType().Name}] runKillCountVar not assigned on {gameObject.name}.", this);
            if (runAbsorptionCountVar == null) Debug.LogWarning($"[{GetType().Name}] runAbsorptionCountVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
