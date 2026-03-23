using UnityEngine;
using Unity.Mathematics;
using Action002.Core;
using Action002.Player.Data;
using Action002.Player.Logic;
using Action002.Input;
using Action002.Visual;
using Tang3cko.ReactiveSO;

namespace Action002.Player.Systems
{
    public class PlayerController : MonoBehaviour, IPlayerGrowthActions
    {
        private Camera gameplayCamera;

        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("Variables (write)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;
        [SerializeField] private IntVariableSO playerHpVar;
        [SerializeField] private IntVariableSO scoreVar;
        [SerializeField] private IntVariableSO comboCountVar;
        [SerializeField] private FloatVariableSO spinGaugeVar;

        [Header("Growth Variables (write)")]
        [SerializeField] private IntVariableSO playerLevelVar;
        [SerializeField] private IntVariableSO playerBulletCountVar;
        [SerializeField] private FloatVariableSO bulletSpeedMultiplierVar;

        [Header("Events (subscribe)")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;
        [SerializeField] private FloatEventChannelSO onComboIncremented;
        [SerializeField] private IntEventChannelSO onKillScoreAdded;
        [SerializeField] private IntEventChannelSO onScoreAdded;
        [SerializeField] private VoidEventChannelSO onForcedPolaritySwitch;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onGameOver;
        [SerializeField] private IntEventChannelSO onPlayerLevelUp;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private PlayerState state;
        private bool hasGameOverFired;
        private Vector2 moveInput;
        private Texture2D generatedTexture;
        private PlayerGrowthCoordinator growthCoordinator;
        private float moveSpeedMultiplier = 1f;
        private bool forcedSwitchPending;

        private void Awake()
        {
            gameplayCamera = Camera.main;
            growthCoordinator = new PlayerGrowthCoordinator(this);
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnMoveEvent += HandleMove;
                inputReader.OnSwitchPolarityEvent += HandleSwitchPolarity;
            }
            if (onPlayerDamaged != null)
                onPlayerDamaged.OnEventRaised += HandleDamage;
            if (onComboIncremented != null)
                onComboIncremented.OnEventRaised += HandleIncrementCombo;
            if (onKillScoreAdded != null)
                onKillScoreAdded.OnEventRaised += HandleAddKillScore;
            if (onScoreAdded != null)
                onScoreAdded.OnEventRaised += HandleAddScore;
            if (onForcedPolaritySwitch != null)
                onForcedPolaritySwitch.OnEventRaised += HandleForcedPolaritySwitch;
        }

        private void Start()
        {
            if (gameConfig == null) return;

            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                generatedTexture = CircleTextureGenerator.Create(64);
                spriteRenderer.sprite = Sprite.Create(generatedTexture,
                    new Rect(0, 0, generatedTexture.width, generatedTexture.height),
                    new Vector2(0.5f, 0.5f), generatedTexture.width);
            }

            state = new PlayerState
            {
                Position = new float2(transform.position.x, transform.position.y),
                CurrentPolarity = Polarity.White,
                Hp = gameConfig.MaxHp,

                ComboMultiplier = 1f,
            };
            hasGameOverFired = false;
            SyncVariables();
            UpdateVisual();
        }

        private void Update()
        {
            if (gameConfig == null) return;

            float2 input = new float2(moveInput.x, moveInput.y);
            float2 velocity = MovementCalculator.CalculateVelocity(input, gameConfig.MoveSpeed * moveSpeedMultiplier);
            state.Position += velocity * Time.deltaTime;

            state.Position = ClampPositionToViewport(state.Position);
            transform.position = new Vector3(state.Position.x, state.Position.y, 0f);
            if (playerPositionVar != null)
                playerPositionVar.Value = new Vector2(state.Position.x, state.Position.y);

            state = DamageCalculator.TickInvincibility(state, Time.deltaTime);

            if (growthCoordinator != null)
            {
                growthCoordinator.CheckAndApplyGrowth(state.SpinGauge);
                SyncVariables();
            }

            var prevCombo = state.ComboCount;
            state = ComboCalculator.TickComboTimer(state, Time.deltaTime);
            if (prevCombo > 0 && state.ComboCount == 0)
                SyncVariables();

            if (spriteRenderer != null)
            {
                float alpha = DamageCalculator.IsInvincible(state)
                    ? (Mathf.Sin(Time.time * 20f) > 0f ? 0.3f : 1f)
                    : 1f;
                var c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            if (!hasGameOverFired && DamageCalculator.IsDead(state))
            {
                hasGameOverFired = true;
                onGameOver?.RaiseEvent();
            }

            if (forcedSwitchPending)
            {
                forcedSwitchPending = false;
                HandleSwitchPolarity();
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnMoveEvent -= HandleMove;
                inputReader.OnSwitchPolarityEvent -= HandleSwitchPolarity;
            }
            if (onPlayerDamaged != null)
                onPlayerDamaged.OnEventRaised -= HandleDamage;
            if (onComboIncremented != null)
                onComboIncremented.OnEventRaised -= HandleIncrementCombo;
            if (onKillScoreAdded != null)
                onKillScoreAdded.OnEventRaised -= HandleAddKillScore;
            if (onScoreAdded != null)
                onScoreAdded.OnEventRaised -= HandleAddScore;
            if (onForcedPolaritySwitch != null)
                onForcedPolaritySwitch.OnEventRaised -= HandleForcedPolaritySwitch;
        }

        private void OnDestroy()
        {
            if (spriteRenderer != null && spriteRenderer.sprite != null && generatedTexture != null)
            {
                var sprite = spriteRenderer.sprite;
                spriteRenderer.sprite = null;
                Destroy(sprite);
                Destroy(generatedTexture);
                generatedTexture = null;
            }
        }

        /// <summary>
        /// AccessoryManager を PlayerGrowthCoordinator に注入する。
        /// GameplaySceneLifetime 等の初期化箇所から呼ばれる。
        /// </summary>
        public void SetAccessoryManager(Accessory.AccessoryManager manager)
        {
            growthCoordinator?.SetAccessoryManager(manager);
        }

        public void ResetForNewRun()
        {
            if (gameConfig == null) return;
            state = new PlayerState
            {
                Position = float2.zero,
                CurrentPolarity = Polarity.White,
                Hp = gameConfig.MaxHp,

                ComboMultiplier = 1f,
            };
            hasGameOverFired = false;
            forcedSwitchPending = false;
            moveInput = Vector2.zero;
            transform.position = Vector3.zero;

            if (growthCoordinator != null)
            {
                growthCoordinator.Reset();
                var defaultGrowth = PlayerGrowthCalculator.CreateDefault();
                ApplyGrowth(defaultGrowth);
                if (playerLevelVar != null)
                    playerLevelVar.Value = defaultGrowth.Level;
            }

            SyncVariables();
            UpdateVisual();
        }

        // --- Input Handlers ---

        private void HandleMove(Vector2 input)
        {
            moveInput = input;
        }

        private void HandleSwitchPolarity()
        {
            state.CurrentPolarity = PolarityCalculator.Toggle(state.CurrentPolarity);
            UpdateVisual();
            if (playerPolarityVar != null)
                playerPolarityVar.Value = (int)state.CurrentPolarity;
        }

        private void HandleForcedPolaritySwitch()
        {
            forcedSwitchPending = true;
        }

        // --- Event Handlers ---

        private void HandleDamage()
        {
            if (gameConfig == null) return;
            if (DamageCalculator.IsInvincible(state)) return;
            state = DamageCalculator.ApplyDamage(state, gameConfig.InvincibleDuration);
            SyncVariables();
            if (!hasGameOverFired && DamageCalculator.IsDead(state))
            {
                hasGameOverFired = true;
                onGameOver?.RaiseEvent();
            }
        }

        private void HandleIncrementCombo(float bulletValue)
        {
            if (gameConfig == null) return;
            state = ComboCalculator.IncrementCombo(
                state, bulletValue, gameConfig.ComboMultiplierStep,
                gameConfig.ComboTimeout, gameConfig.AbsorbGaugeRate);
            SyncVariables();
        }

        private void HandleAddKillScore(int baseScore)
        {
            if (gameConfig == null) return;
            state = ScoreCalculator.AddKillScore(state, baseScore, gameConfig.KillGaugeRate);
            SyncVariables();
        }

        private void HandleAddScore(int amount)
        {
            state.Score += amount;
            SyncVariables();
        }

        // --- IPlayerGrowthActions ---

        void IPlayerGrowthActions.ResetSpinGauge()
        {
            state.SpinGauge = 0f;
        }

        void IPlayerGrowthActions.ApplyGrowth(PlayerGrowthState growthState)
        {
            ApplyGrowth(growthState);
        }

        void IPlayerGrowthActions.RaiseLevelUp(int level)
        {
            if (playerLevelVar != null)
                playerLevelVar.Value = level;
            onPlayerLevelUp?.RaiseEvent(level);
        }

        private void ApplyGrowth(PlayerGrowthState growthState)
        {
            moveSpeedMultiplier = growthState.MoveSpeedMultiplier;
            if (playerBulletCountVar != null)
                playerBulletCountVar.Value = growthState.BulletCount;
            if (bulletSpeedMultiplierVar != null)
                bulletSpeedMultiplierVar.Value = growthState.BulletSpeedMultiplier;
        }

        // --- State ---

        private void SyncVariables()
        {
            if (playerPositionVar != null)
                playerPositionVar.Value = new Vector2(state.Position.x, state.Position.y);
            if (playerHpVar != null) playerHpVar.Value = state.Hp;
            if (scoreVar != null) scoreVar.Value = state.Score;
            if (comboCountVar != null) comboCountVar.Value = state.ComboCount;
            if (spinGaugeVar != null) spinGaugeVar.Value = state.SpinGauge;
            if (playerPolarityVar != null)
                playerPolarityVar.Value = (int)state.CurrentPolarity;
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = PolarityColors.GetForeground(state.CurrentPolarity);
        }

        private float2 ClampPositionToViewport(float2 position)
        {
            if (gameplayCamera == null)
                return position;

            float cameraDistance = Mathf.Abs(gameplayCamera.transform.position.z - transform.position.z);
            Vector3 min = gameplayCamera.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDistance));
            Vector3 max = gameplayCamera.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDistance));

            return MovementCalculator.ClampPosition(
                position,
                new float2(min.x, min.y),
                new float2(max.x, max.y));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (inputReader == null) Debug.LogWarning($"[{GetType().Name}] inputReader not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
            if (playerHpVar == null) Debug.LogWarning($"[{GetType().Name}] playerHpVar not assigned on {gameObject.name}.", this);
            if (scoreVar == null) Debug.LogWarning($"[{GetType().Name}] scoreVar not assigned on {gameObject.name}.", this);
            if (comboCountVar == null) Debug.LogWarning($"[{GetType().Name}] comboCountVar not assigned on {gameObject.name}.", this);
            if (spinGaugeVar == null) Debug.LogWarning($"[{GetType().Name}] spinGaugeVar not assigned on {gameObject.name}.", this);
            if (onPlayerDamaged == null) Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
            if (onComboIncremented == null) Debug.LogWarning($"[{GetType().Name}] onComboIncremented not assigned on {gameObject.name}.", this);
            if (onKillScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onKillScoreAdded not assigned on {gameObject.name}.", this);
            if (onScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onScoreAdded not assigned on {gameObject.name}.", this);
            if (onGameOver == null) Debug.LogWarning($"[{GetType().Name}] onGameOver not assigned on {gameObject.name}.", this);
            if (onPlayerLevelUp == null) Debug.LogWarning($"[{GetType().Name}] onPlayerLevelUp not assigned on {gameObject.name}.", this);
            if (playerLevelVar == null) Debug.LogWarning($"[{GetType().Name}] playerLevelVar not assigned on {gameObject.name}.", this);
            if (playerBulletCountVar == null) Debug.LogWarning($"[{GetType().Name}] playerBulletCountVar not assigned on {gameObject.name}.", this);
            if (bulletSpeedMultiplierVar == null) Debug.LogWarning($"[{GetType().Name}] bulletSpeedMultiplierVar not assigned on {gameObject.name}.", this);
            if (spriteRenderer == null) Debug.LogWarning($"[{GetType().Name}] spriteRenderer not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
