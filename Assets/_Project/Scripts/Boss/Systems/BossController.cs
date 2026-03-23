using UnityEngine;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Boss.Data;
using Action002.Boss.Logic;
using Action002.Boss.Rendering;
using Action002.Bullet.Data;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Boss.Systems
{
    public class BossController : MonoBehaviour, IBossAIActions, IBossHitTarget
    {
        [Header("Config")]
        [SerializeField] private BossConfigSO bossConfig;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;

        private BossRenderer bossRenderer;

        [Header("Sets")]
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("Events (subscribe)")]
        [SerializeField] private VoidEventChannelSO onBossPhaseRequested;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onBossDefeated;
        [SerializeField] private GameObjectEventChannelSO onBossHitTargetChanged;
        [SerializeField] private IntEventChannelSO onScoreAdded;
        [SerializeField] private VoidEventChannelSO onForcedPolaritySwitch;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;

        private BossAI ai;
        private int nextBulletId = 200000;

        // --- IBossHitTarget ---

        public bool IsActive =>
            ai != null && ai.IsActive &&
            (ai.CurrentPhase == BossPhaseId.Phase1 || ai.CurrentPhase == BossPhaseId.Phase2);

        public int EntityCount => 3;

        public bool GetEntityInfo(int index, out float2 position,
            out float collisionRadius, out bool isActive)
        {
            if (ai == null || index < 0 || index >= 3)
            {
                position = float2.zero;
                collisionRadius = 0f;
                isActive = false;
                return false;
            }

            var entity = ai.GetEntity((BossEntityId)index);
            position = entity.Position;
            collisionRadius = entity.CollisionRadius;
            isActive = entity.IsActive;
            return true;
        }

        public bool TryApplyDamageToEntity(int entityIndex, int damage)
        {
            if (ai == null || !ai.IsActive) return false;
            if (entityIndex < 0 || entityIndex >= 3) return false;

            var entityId = (BossEntityId)entityIndex;
            var entity = ai.GetEntity(entityId);
            if (!entity.IsActive) return false;

            ai.ApplyDamage(entityId, damage);
            return true;
        }

        public bool TryHitAny(float bulletX, float bulletY, float bulletRadius, int damage)
        {
            if (ai == null || !ai.IsActive) return false;

            for (int i = 0; i < 3; i++)
            {
                var entity = ai.GetEntity((BossEntityId)i);
                if (!entity.IsActive) continue;

                float combinedRadius = bulletRadius + entity.CollisionRadius;
                if (BossCollisionCalculator.IsWithinRadius(
                    bulletX, bulletY, entity.Position.x, entity.Position.y, combinedRadius))
                {
                    ai.ApplyDamage((BossEntityId)i, damage);
                    return true;
                }
            }
            return false;
        }

        // --- MonoBehaviour ---

        private void Awake()
        {
            bossRenderer = GetComponent<BossRenderer>();
        }

        private void OnEnable()
        {
            if (onBossPhaseRequested != null)
                onBossPhaseRequested.OnEventRaised += HandleBossPhaseRequested;
        }

        private void OnDisable()
        {
            if (onBossPhaseRequested != null)
                onBossPhaseRequested.OnEventRaised -= HandleBossPhaseRequested;

            onBossHitTargetChanged?.RaiseEvent(null);
        }

        private void Update()
        {
            if (ai == null || !ai.IsActive) return;
            ai.Tick(Time.deltaTime);
        }

        private void HandleBossPhaseRequested()
        {
            if (bossConfig == null)
            {
                Debug.LogError($"[{GetType().Name}] bossConfig not assigned. Cannot start boss phase.", this);
                return;
            }

            if (rhythmClock == null)
            {
                Debug.LogError($"[{GetType().Name}] rhythmClock not assigned. Cannot start boss phase.", this);
                return;
            }

            ai = new BossAI(
                this,
                rhythmClock,
                phase1HpPerGuardian: bossConfig.Phase1HpPerGuardian,
                phase2HpMagatama: bossConfig.Phase2HpMagatama,
                phase1ShootCooldown: bossConfig.Phase1ShootCooldown,
                phase2ShootCooldown: bossConfig.Phase2ShootCooldown,
                phase1SimultaneousThreshold: bossConfig.Phase1SimultaneousThreshold,
                phase1ForcedSwitchInterval: bossConfig.Phase1ForcedSwitchInterval,
                phase2ForcedSwitchInterval: bossConfig.Phase2ForcedSwitchInterval,
                forcedSwitchWarningDuration: bossConfig.ForcedSwitchWarningDuration,
                introDuration: bossConfig.IntroDuration,
                mergeDuration: bossConfig.MergeDuration,
                phase1KillScore: bossConfig.Phase1KillScore,
                phase2KillScore: bossConfig.Phase2KillScore,
                guardianCollisionRadius: bossConfig.GuardianCollisionRadius,
                magatamaCollisionRadius: bossConfig.MagatamaCollisionRadius,
                whiteGuardianOffset: bossConfig.WhiteGuardianOffset,
                blackGuardianOffset: bossConfig.BlackGuardianOffset,
                magatamaRotationSpeed: bossConfig.MagatamaRotationSpeed);

            if (bossRenderer != null)
                bossRenderer.HideAll();

            ai.Begin(float2.zero);

            onBossHitTargetChanged?.RaiseEvent(gameObject);
        }

        // --- IBossAIActions ---

        void IBossAIActions.SpawnEntity(BossEntityId id, float x, float y, int hp, byte polarity, float collisionRadius)
        {
            if (bossRenderer == null) return;
            bossRenderer.ShowEntity(id);
            bossRenderer.UpdatePosition(id, x, y);
            bossRenderer.UpdatePolarity(id, polarity);
        }

        void IBossAIActions.DespawnEntity(BossEntityId id)
        {
            if (bossRenderer != null)
                bossRenderer.HideEntity(id);
        }

        void IBossAIActions.SetEntityPosition(BossEntityId id, float x, float y)
        {
            if (bossRenderer != null)
                bossRenderer.UpdatePosition(id, x, y);
        }

        void IBossAIActions.SetEntityActive(BossEntityId id, bool active)
        {
            if (bossRenderer == null) return;
            if (active) bossRenderer.ShowEntity(id);
            else bossRenderer.HideEntity(id);
        }

        void IBossAIActions.FireBullets(BossEntityId sourceId, float x, float y, byte polarity, int patternIndex)
        {
            if (bulletSet == null) return;

            float2 origin = new float2(x, y);
            float2 playerPos = playerPositionVar != null
                ? new float2(playerPositionVar.Value.x, playerPositionVar.Value.y)
                : float2.zero;

            float2 dir = playerPos - origin;
            float dist = math.length(dir);
            if (dist < 0.01f) dir = new float2(0f, -1f);
            else dir /= dist;

            float baseAngle = math.atan2(dir.y, dir.x);

            if (patternIndex == 0)
            {
                int count = 5;
                float arc = math.radians(30f);
                float speed = 4f;
                float startAngle = baseAngle - arc * 0.5f;
                float step = count > 1 ? arc / (count - 1) : 0f;

                for (int i = 0; i < count; i++)
                {
                    float angle = startAngle + step * i;
                    var bullet = new BulletState
                    {
                        Position = origin,
                        Velocity = new float2(math.cos(angle), math.sin(angle)) * speed,
                        ScoreValue = 1f,
                        Polarity = polarity,
                        Faction = BulletFaction.Enemy,
                        Damage = 1,
                    };
                    bulletSet.Register(nextBulletId++, bullet);
                }
            }
            else if (patternIndex == 1)
            {
                int arms = 6;
                float speed = 3.5f;
                float spiralAngle = Time.time * 0.5f;

                for (int i = 0; i < arms; i++)
                {
                    float angle = spiralAngle + (i / (float)arms) * math.PI * 2f;
                    byte armPolarity = (byte)(i % 2 == 0 ? 0 : 1);
                    var bullet = new BulletState
                    {
                        Position = origin,
                        Velocity = new float2(math.cos(angle), math.sin(angle)) * speed,
                        ScoreValue = 1f,
                        Polarity = armPolarity,
                        Faction = BulletFaction.Enemy,
                        Damage = 1,
                    };
                    bulletSet.Register(nextBulletId++, bullet);
                }
            }
        }

        void IBossAIActions.RequestForcedPolaritySwitch()
        {
            onForcedPolaritySwitch?.RaiseEvent();
        }

        void IBossAIActions.AddScore(int amount)
        {
            onScoreAdded?.RaiseEvent(amount);
        }

        void IBossAIActions.NotifyBossDefeated()
        {
            onBossDefeated?.RaiseEvent();
        }

        void IBossAIActions.PlayMergeAnimation()
        {
            if (bossRenderer != null)
                bossRenderer.PlayMergeAnimation();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bossConfig == null) Debug.LogWarning($"[{GetType().Name}] bossConfig not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (onBossPhaseRequested == null) Debug.LogWarning($"[{GetType().Name}] onBossPhaseRequested not assigned on {gameObject.name}.", this);
            if (onBossDefeated == null) Debug.LogWarning($"[{GetType().Name}] onBossDefeated not assigned on {gameObject.name}.", this);
            if (onScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onScoreAdded not assigned on {gameObject.name}.", this);
            if (onForcedPolaritySwitch == null) Debug.LogWarning($"[{GetType().Name}] onForcedPolaritySwitch not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (onBossHitTargetChanged == null) Debug.LogWarning($"[{GetType().Name}] onBossHitTargetChanged not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
