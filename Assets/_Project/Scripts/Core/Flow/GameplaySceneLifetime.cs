using UnityEngine;
using Action002.Accessory;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Systems;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Action002.Enemy.Systems;
using Action002.Input;
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Core.Flow
{
    public class GameplaySceneLifetime : MonoBehaviour, IGameplayStartupActions
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemyStateSet;
        [SerializeField] private BulletStateSetSO bulletStateSet;

        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("Systems")]
        [SerializeField] private GameLoopManager gameLoopManager;
        [SerializeField] private RhythmClockSystem rhythmClockSystem;
        [SerializeField] private EnemySpawnSystem enemySpawnSystem;
        [SerializeField] private SonicWaveSystem sonicWaveSystem;
        [SerializeField] private WaveCollisionSystem waveCollisionSystem;
        [SerializeField] private PlayerController playerController;

        [Header("Sets (Wave)")]
        [SerializeField] private WaveStateSetSO waveStateSet;

        private bool startupResetFailed;

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
        [SerializeField] private IntVariableSO playerLevelVar;
        [SerializeField] private IntVariableSO playerBulletCountVar;
        [SerializeField] private FloatVariableSO bulletSpeedMultiplierVar;

        private void Awake()
        {
            if (enemyStateSet != null) enemyStateSet.Clear();
            if (bulletStateSet != null) bulletStateSet.Clear();
            if (waveStateSet != null) waveStateSet.Clear();

            if (playerHpVar != null) playerHpVar.ResetToInitial();
            if (scoreVar != null) scoreVar.ResetToInitial();
            if (comboCountVar != null) comboCountVar.ResetToInitial();
            if (spinGaugeVar != null) spinGaugeVar.ResetToInitial();
            if (playerPositionVar != null) playerPositionVar.ResetToInitial();
            if (playerPolarityVar != null) playerPolarityVar.ResetToInitial();
            if (maxComboVar != null) maxComboVar.ResetToInitial();
            if (runKillCountVar != null) runKillCountVar.ResetToInitial();
            if (runAbsorptionCountVar != null) runAbsorptionCountVar.ResetToInitial();
            if (playerLevelVar != null) playerLevelVar.ResetToInitial();
            if (playerBulletCountVar != null) playerBulletCountVar.ResetToInitial();
            if (bulletSpeedMultiplierVar != null) bulletSpeedMultiplierVar.ResetToInitial();
        }

        private void Start()
        {
            // AccessoryManager を作成し、SonicWave を登録して PlayerController に注入
            SetupAccessoryManager();

            var logic = new GameplayStartupLogic(this);
            logic.Execute();
        }

        private void SetupAccessoryManager()
        {
            var accessoryManager = new AccessoryManager();

            // SonicWave の IAccessory を登録
            if (sonicWaveSystem != null && sonicWaveSystem.Accessory != null)
                accessoryManager.Register(sonicWaveSystem.Accessory);

            // PlayerController に注入
            if (playerController != null)
                playerController.SetAccessoryManager(accessoryManager);
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
            startupResetFailed = false;

            if (gameConfig == null)
            {
                Debug.LogError("[GameplaySceneLifetime] gameConfig is not assigned. Cannot resolve run seed.", this);
                startupResetFailed = true;
                return;
            }

            uint runSeed = SeedHelper.ResolveRunSeed(
                gameConfig.FixedRunSeed,
                (uint)System.DateTime.Now.Ticks);
            if (enemySpawnSystem != null) enemySpawnSystem.ResetForNewRun(runSeed);
            if (rhythmClockSystem != null) rhythmClockSystem.ResetForNewRun();
            if (sonicWaveSystem != null) sonicWaveSystem.ResetForNewRun();
            if (waveCollisionSystem != null) waveCollisionSystem.ResetForNewRun();
            if (waveStateSet != null) waveStateSet.Clear();
        }

        bool IGameplayStartupActions.StartClock()
        {
            if (startupResetFailed)
            {
                Debug.LogError("[GameplaySceneLifetime] Startup reset failed. Cannot start clock.", this);
                return false;
            }

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
            if (waveStateSet != null) waveStateSet.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (enemyStateSet == null) Debug.LogWarning($"[{GetType().Name}] enemyStateSet not assigned on {gameObject.name}.", this);
            if (bulletStateSet == null) Debug.LogWarning($"[{GetType().Name}] bulletStateSet not assigned on {gameObject.name}.", this);
            if (inputReader == null) Debug.LogWarning($"[{GetType().Name}] inputReader not assigned on {gameObject.name}.", this);
            if (gameLoopManager == null) Debug.LogWarning($"[{GetType().Name}] gameLoopManager not assigned on {gameObject.name}.", this);
            if (rhythmClockSystem == null) Debug.LogWarning($"[{GetType().Name}] rhythmClockSystem not assigned on {gameObject.name}.", this);
            if (enemySpawnSystem == null) Debug.LogWarning($"[{GetType().Name}] enemySpawnSystem not assigned on {gameObject.name}.", this);
            if (playerController == null) Debug.LogWarning($"[{GetType().Name}] playerController not assigned on {gameObject.name}.", this);
            if (playerHpVar == null) Debug.LogWarning($"[{GetType().Name}] playerHpVar not assigned on {gameObject.name}.", this);
            if (scoreVar == null) Debug.LogWarning($"[{GetType().Name}] scoreVar not assigned on {gameObject.name}.", this);
            if (comboCountVar == null) Debug.LogWarning($"[{GetType().Name}] comboCountVar not assigned on {gameObject.name}.", this);
            if (spinGaugeVar == null) Debug.LogWarning($"[{GetType().Name}] spinGaugeVar not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
            if (maxComboVar == null) Debug.LogWarning($"[{GetType().Name}] maxComboVar not assigned on {gameObject.name}.", this);
            if (runKillCountVar == null) Debug.LogWarning($"[{GetType().Name}] runKillCountVar not assigned on {gameObject.name}.", this);
            if (runAbsorptionCountVar == null) Debug.LogWarning($"[{GetType().Name}] runAbsorptionCountVar not assigned on {gameObject.name}.", this);
            if (playerLevelVar == null) Debug.LogWarning($"[{GetType().Name}] playerLevelVar not assigned on {gameObject.name}.", this);
            if (playerBulletCountVar == null) Debug.LogWarning($"[{GetType().Name}] playerBulletCountVar not assigned on {gameObject.name}.", this);
            if (bulletSpeedMultiplierVar == null) Debug.LogWarning($"[{GetType().Name}] bulletSpeedMultiplierVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
