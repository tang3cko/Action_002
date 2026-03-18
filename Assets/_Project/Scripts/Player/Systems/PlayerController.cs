using UnityEngine;
using Unity.Mathematics;
using Action002.Core;
using Action002.Player.Data;
using Action002.Player.Logic;
using Action002.Input;
using Tang3cko.ReactiveSO;

namespace Action002.Player.Systems
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("Variables (write)")]
        [SerializeField] private IntVariableSO playerPolarityVar;
        [SerializeField] private IntVariableSO playerHpVar;
        [SerializeField] private IntVariableSO scoreVar;
        [SerializeField] private IntVariableSO comboCountVar;
        [SerializeField] private FloatVariableSO spinGaugeVar;

        [Header("Events")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;
        [SerializeField] private VoidEventChannelSO onGameOver;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private PlayerState state;
        private bool _gameOverFired;
        private Vector2 moveInput;
        private Vector2 playAreaMin = new Vector2(-17f, -9.5f);
        private Vector2 playAreaMax = new Vector2(17f, 9.5f);

        public float2 Position => state.Position;
        public Polarity CurrentPolarity => state.CurrentPolarity;
        // NOTE: Exposes mutable ref for performance (used by BulletCollisionSystem).
        // Consider replacing with read-only accessors if mutation is not needed externally.
        public ref PlayerState State => ref state;

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnMoveEvent += HandleMove;
                inputReader.OnSwitchPolarityEvent += HandleSwitchPolarity;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnMoveEvent -= HandleMove;
                inputReader.OnSwitchPolarityEvent -= HandleSwitchPolarity;
            }
        }

        private void Start()
        {
            if (gameConfig == null) return;

            state = new PlayerState
            {
                Position = new float2(transform.position.x, transform.position.y),
                CurrentPolarity = Polarity.White,
                Hp = gameConfig.MaxHp,
                MaxHp = gameConfig.MaxHp,
                ComboMultiplier = 1f,
            };
            _gameOverFired = false;
            SyncVariables();
            UpdateVisual();
        }

        private void Update()
        {
            if (gameConfig == null) return;

            // Movement
            float2 input = new float2(moveInput.x, moveInput.y);
            float2 velocity = math.normalizesafe(input) * gameConfig.MoveSpeed;
            state.Position += velocity * Time.deltaTime;

            // Clamp
            state.Position = math.clamp(state.Position, new float2(playAreaMin.x, playAreaMin.y), new float2(playAreaMax.x, playAreaMax.y));
            transform.position = new Vector3(state.Position.x, state.Position.y, 0f);

            // Invincibility
            state = DamageCalculator.TickInvincibility(state, Time.deltaTime);

            // Combo timeout
            if (state.ComboCount > 0)
            {
                state.ComboTimer -= Time.deltaTime;
                if (state.ComboTimer <= 0f)
                {
                    state.ComboCount = 0;
                    state.ComboMultiplier = 1f;
                    SyncVariables();
                }
            }

            // Invincibility visual
            if (spriteRenderer != null)
            {
                float alpha = DamageCalculator.IsInvincible(state) ? (Mathf.Sin(Time.time * 20f) > 0f ? 0.3f : 1f) : 1f;
                var c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            // Death check
            if (!_gameOverFired && DamageCalculator.IsDead(state))
            {
                _gameOverFired = true;
                onGameOver?.RaiseEvent();
            }
        }

        private void HandleMove(Vector2 input)
        {
            moveInput = input;
        }

        private void HandleSwitchPolarity()
        {
            state.CurrentPolarity = PolarityCalculator.Toggle(state.CurrentPolarity);
            UpdateVisual();
            if (playerPolarityVar != null) playerPolarityVar.Value = (int)state.CurrentPolarity;
            onPolarityChanged?.RaiseEvent((int)state.CurrentPolarity);
        }

        public bool CheckDeathImmediate()
        {
            if (!_gameOverFired && DamageCalculator.IsDead(state))
            {
                _gameOverFired = true;
                onGameOver?.RaiseEvent();
                return true;
            }
            return false;
        }

        public void ApplyDamage()
        {
            if (gameConfig == null) return;
            state = DamageCalculator.ApplyDamage(state, gameConfig.InvincibleDuration);
            SyncVariables();
        }

        public void AddScore(int amount)
        {
            state.Score += amount;
            SyncVariables();
        }

        public void IncrementCombo(float bulletValue)
        {
            if (gameConfig == null) return;
            state.ComboCount++;
            state.ComboMultiplier = Bullet.Logic.AbsorptionCalculator.CalculateComboMultiplier(state.ComboCount, gameConfig.ComboMultiplierStep);
            state.ComboTimer = gameConfig.ComboTimeout;
            state.SpinGauge = math.min(1f, state.SpinGauge + gameConfig.AbsorbGaugeRate);

            int absorbScore = (int)Bullet.Logic.AbsorptionCalculator.CalculateAbsorbScore(bulletValue, state.ComboMultiplier);
            state.Score += absorbScore;
            SyncVariables();
        }

        public void AddKillScore(int baseScore)
        {
            if (gameConfig == null) return;
            state.Score += baseScore;
            state.SpinGauge = math.min(1f, state.SpinGauge + gameConfig.KillGaugeRate);
            SyncVariables();
        }

        private void SyncVariables()
        {
            if (playerHpVar != null) playerHpVar.Value = state.Hp;
            if (scoreVar != null) scoreVar.Value = state.Score;
            if (comboCountVar != null) comboCountVar.Value = state.ComboCount;
            if (spinGaugeVar != null) spinGaugeVar.Value = state.SpinGauge;
            if (playerPolarityVar != null) playerPolarityVar.Value = (int)state.CurrentPolarity;
        }

        public void ResetForNewRun()
        {
            if (gameConfig == null) return;
            state = new PlayerState
            {
                Position = float2.zero,
                CurrentPolarity = Polarity.White,
                Hp = gameConfig.MaxHp,
                MaxHp = gameConfig.MaxHp,
                ComboMultiplier = 1f,
            };
            _gameOverFired = false;
            moveInput = Vector2.zero;
            transform.position = Vector3.zero;
            SyncVariables();
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = state.CurrentPolarity == Polarity.White
                ? new Color(0.878f, 0.878f, 1f)
                : new Color(0.15f, 0.15f, 0.25f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (inputReader == null) Debug.LogWarning($"[{GetType().Name}] inputReader not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
