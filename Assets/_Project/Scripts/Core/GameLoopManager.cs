using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Logic;
using Action002.Accessory.SonicWave.Systems;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Bullet.Systems;
using Action002.Core.Flow;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Action002.Enemy.Systems;
using Action002.Audio.Systems;
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Core
{
    public class GameLoopManager : MonoBehaviour
    {
        [Header("Config")]
        private Camera mainCamera;
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private BulletStateSetSO bulletSet;
        [SerializeField] private WaveStateSetSO waveSet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;
        [SerializeField] private IntVariableSO playerHpVar;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;
        [SerializeField] private PlayerAttackSystem playerAttack;
        [SerializeField] private BulletCollisionSystem bulletCollision;
        [SerializeField] private EnemySpawnSystem enemySpawn;
        [SerializeField] private EnemyShootSystem enemyShoot;
        [SerializeField] private SonicWaveSystem sonicWave;
        [SerializeField] private WaveCollisionSystem waveCollision;

        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onGamePhaseChanged;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;
        [SerializeField] private IntEventChannelSO onScoreAdded;

        private ReactiveEntitySetOrchestrator<EnemyState> enemyOrchestrator;
        private ReactiveEntitySetOrchestrator<BulletState> bulletOrchestrator;
        private ReactiveEntitySetOrchestrator<WaveState> waveOrchestrator;
        private bool hasPendingEnemyJob;
        private bool hasPendingBulletJob;
        private bool hasPendingWaveJob;
        private List<int> waveDespawnQueue = new List<int>(16);
        private List<int> despawnQueue = new List<int>(256);
        private List<int> enemyDespawnQueue = new List<int>(64);
        private List<int> sameContactIds = new List<int>(64);
        private EnemyContactSessionTracker contactTracker = new EnemyContactSessionTracker();
        private bool isRunning;
        private bool isBossPhase;
        private NativeArray<MovementSpec> movementSpecs;

        // --- Unity Lifecycle ---

        private void Start()
        {
            mainCamera = Camera.main;

            if (enemySet == null || bulletSet == null) return;

            enemyOrchestrator = new ReactiveEntitySetOrchestrator<EnemyState>(enemySet);
            bulletOrchestrator = new ReactiveEntitySetOrchestrator<BulletState>(bulletSet);

            if (waveSet != null)
                waveOrchestrator = new ReactiveEntitySetOrchestrator<WaveState>(waveSet);

            // EnemyTypeId の値の数だけ MovementSpec を構築
            int typeCount = System.Enum.GetValues(typeof(EnemyTypeId)).Length;
            movementSpecs = new NativeArray<MovementSpec>(typeCount, Allocator.Persistent);
            for (int i = 0; i < typeCount; i++)
            {
                var spec = EnemyTypeTable.Get((EnemyTypeId)i);
                movementSpecs[i] = new MovementSpec
                {
                    Pattern = spec.Movement,
                    KeepDistance = spec.KeepDistance,
                    ArrivalThreshold = spec.ArrivalThreshold,
                    RotationSpeed = EnemyRotationCalculator.GetRotationSpeed((EnemyTypeId)i),
                    StepAngle = EnemyRotationCalculator.GetStepAngle((EnemyTypeId)i),
                    HoldRatio = EnemyRotationCalculator.GetHoldRatio((EnemyTypeId)i),
                };
            }

            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised += HandleGamePhaseChanged;
        }

        private void Update()
        {
            if (!isRunning) return;

            ScheduleEnemyJob();
            ScheduleBulletJob();
            ScheduleWaveExpandJob();
        }

        private void LateUpdate()
        {
            if (!isRunning) return;

            if (hasPendingEnemyJob)
            {
                enemyOrchestrator.CompleteAndApply();
                hasPendingEnemyJob = false;
            }

            if (hasPendingBulletJob)
            {
                bulletOrchestrator.CompleteAndApply();
                hasPendingBulletJob = false;
            }

            if (hasPendingWaveJob)
            {
                waveOrchestrator.CompleteAndApply();
                hasPendingWaveJob = false;
            }

            RemoveOffscreenBullets();

            if (rhythmClock != null)
                rhythmClock.ProcessClock();

            if (playerAttack != null)
                playerAttack.ProcessAttacks();

            if (sonicWave != null)
                sonicWave.ProcessAttacks();

            if (bulletCollision != null)
                bulletCollision.ProcessCollisions();

            if (waveCollision != null)
                waveCollision.ProcessCollisions();

            RemoveExpiredWaves();

            ProcessEnemyContacts();

            // Stop spawning if player is dead or boss phase active
            if (playerHpVar != null && playerHpVar.Value <= 0) return;
            if (isBossPhase) return;

            if (enemySpawn != null)
                enemySpawn.ProcessSpawning();
            if (enemyShoot != null)
                enemyShoot.ProcessShooting();
        }

        private void OnDestroy()
        {
            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised -= HandleGamePhaseChanged;

            if (hasPendingEnemyJob)
            {
                enemyOrchestrator?.CompleteAndApply();
                hasPendingEnemyJob = false;
            }
            if (hasPendingBulletJob)
            {
                bulletOrchestrator?.CompleteAndApply();
                hasPendingBulletJob = false;
            }
            if (hasPendingWaveJob)
            {
                waveOrchestrator?.CompleteAndApply();
                hasPendingWaveJob = false;
            }
            enemyOrchestrator?.Dispose();
            enemyOrchestrator = null;
            bulletOrchestrator?.Dispose();
            bulletOrchestrator = null;
            waveOrchestrator?.Dispose();
            waveOrchestrator = null;

            if (movementSpecs.IsCreated)
                movementSpecs.Dispose();
        }

        // --- Public Methods ---

        public void SetRunning(bool running)
        {
            isRunning = running;
        }

        private void HandleGamePhaseChanged(int phase)
        {
            if ((GamePhase)phase == GamePhase.Boss)
            {
                isBossPhase = true;
                ClearForBossPhase();
            }
            else
            {
                isBossPhase = false;
            }
        }

        private void ClearForBossPhase()
        {
            if (enemySet != null)
            {
                var ids = enemySet.EntityIds;
                var idList = new List<int>(enemySet.Count);
                for (int i = 0; i < ids.Length; i++)
                    idList.Add(ids[i]);
                foreach (var id in idList)
                    enemySet.Unregister(id);
            }

            if (bulletSet != null)
            {
                var data = bulletSet.Data;
                var ids = bulletSet.EntityIds;
                var bulletDespawnList = new List<int>(bulletSet.Count);
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].Faction == BulletFaction.Enemy)
                        bulletDespawnList.Add(ids[i]);
                }
                foreach (var id in bulletDespawnList)
                    bulletSet.Unregister(id);
            }
        }

        public void StopAndCleanup()
        {
            isRunning = false;

            if (rhythmClock != null)
                rhythmClock.StopClock();

            if (hasPendingEnemyJob)
            {
                enemyOrchestrator?.CompleteAndApply();
                hasPendingEnemyJob = false;
            }
            if (hasPendingBulletJob)
            {
                bulletOrchestrator?.CompleteAndApply();
                hasPendingBulletJob = false;
            }
            if (hasPendingWaveJob)
            {
                waveOrchestrator?.CompleteAndApply();
                hasPendingWaveJob = false;
            }
        }

        // --- Private Methods ---

        private void ProcessEnemyContacts()
        {
            if (enemySet == null || enemySet.Count == 0 || gameConfig == null) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            sameContactIds.Clear();
            enemyDespawnQueue.Clear();

            var data = enemySet.Data;
            var ids = enemySet.EntityIds;
            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            var playerPolarity = (Polarity)playerPolarityVar.Value;
            float playerContactRadius = gameConfig.PlayerContactRadius;

            for (int i = 0; i < data.Length; i++)
            {
                var enemy = data[i];
                float enemyCollisionRadius = EnemyTypeTable.Get(enemy.TypeId).CollisionRadius;
                if (!EnemyContactCalculator.IsContact(playerPos, enemy.Position, playerContactRadius, enemyCollisionRadius))
                    continue;

                var contactResult = ContactRuleCalculator.Resolve(playerPolarity, enemy.Polarity);

                if (ContactRuleCalculator.IsDamageContact(contactResult))
                {
                    onPlayerDamaged?.RaiseEvent();
                    enemyDespawnQueue.Add(ids[i]);
                }
                else
                {
                    sameContactIds.Add(ids[i]);
                }
            }

            var newContacts = contactTracker.UpdateContacts(sameContactIds);
            foreach (var id in newContacts)
            {
                // newContacts = first-frame contacts only (tracked by SessionTracker)
                // IsScoringContact encodes the rule: same polarity + first contact = score
                onScoreAdded?.RaiseEvent(gameConfig.ContactScoreBonus);
            }

            foreach (var id in enemyDespawnQueue)
            {
                enemySet.Unregister(id);
            }
        }

        private void ScheduleEnemyJob()
        {
            if (enemySet == null || enemySet.Count == 0) return;
            if (playerPositionVar == null || enemyOrchestrator == null) return;

            var src = enemySet.Data;
            var dst = enemyOrchestrator.GetBackBuffer();

            var job = new EnemyMoveJob
            {
                Src = src,
                Dst = dst,
                PlayerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y),
                DeltaTime = Time.deltaTime,
                TypeSpecs = movementSpecs,
            };

            var handle = job.Schedule(enemySet.Count, 64);
            enemyOrchestrator.ScheduleUpdate(handle, enemySet.Count);
            hasPendingEnemyJob = true;
        }

        private void ScheduleBulletJob()
        {
            if (bulletSet == null || bulletSet.Count == 0) return;
            if (bulletOrchestrator == null) return;

            var src = bulletSet.Data;
            var dst = bulletOrchestrator.GetBackBuffer();

            var job = new BulletMoveJob
            {
                Src = src,
                Dst = dst,
                DeltaTime = Time.deltaTime,
            };

            var handle = job.Schedule(bulletSet.Count, 64);
            bulletOrchestrator.ScheduleUpdate(handle, bulletSet.Count);
            hasPendingBulletJob = true;
        }

        private void RemoveOffscreenBullets()
        {
            if (bulletSet == null || bulletSet.Count == 0) return;
            if (gameConfig == null || mainCamera == null) return;

            despawnQueue.Clear();
            if (despawnQueue.Capacity < bulletSet.Count)
                despawnQueue.Capacity = bulletSet.Count;

            float margin = gameConfig.BulletOffscreenMargin;
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
            float2 min = new float2(bottomLeft.x, bottomLeft.y);
            float2 max = new float2(topRight.x, topRight.y);

            var data = bulletSet.Data;
            var ids = bulletSet.EntityIds;

            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (BulletBoundsCalculator.IsOutsideBounds(data[i].Position, min, max, margin))
                {
                    despawnQueue.Add(ids[i]);
                }
            }

            foreach (var id in despawnQueue)
            {
                bulletSet.Unregister(id);
            }
        }

        private void ScheduleWaveExpandJob()
        {
            if (waveSet == null || waveSet.Count == 0) return;
            if (waveOrchestrator == null) return;

            var src = waveSet.Data;
            var dst = waveOrchestrator.GetBackBuffer();

            var job = new WaveExpandJob
            {
                Src = src,
                Dst = dst,
                DeltaTime = Time.deltaTime,
            };

            var handle = job.Schedule(waveSet.Count, 64);
            waveOrchestrator.ScheduleUpdate(handle, waveSet.Count);
            hasPendingWaveJob = true;
        }

        private void RemoveExpiredWaves()
        {
            if (waveSet == null || waveSet.Count == 0) return;
            waveDespawnQueue.Clear();

            var data = waveSet.Data;
            var ids = waveSet.EntityIds;
            for (int i = 0; i < data.Length; i++)
            {
                if (WaveBoundsCalculator.IsExpired(data[i].ElapsedTime, data[i].Duration))
                    waveDespawnQueue.Add(ids[i]);
            }

            foreach (var id in waveDespawnQueue)
                waveSet.Unregister(id);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
            if (playerHpVar == null) Debug.LogWarning($"[{GetType().Name}] playerHpVar not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (playerAttack == null) Debug.LogWarning($"[{GetType().Name}] playerAttack not assigned on {gameObject.name}.", this);
            if (bulletCollision == null) Debug.LogWarning($"[{GetType().Name}] bulletCollision not assigned on {gameObject.name}.", this);
            if (enemySpawn == null) Debug.LogWarning($"[{GetType().Name}] enemySpawn not assigned on {gameObject.name}.", this);
            if (enemyShoot == null) Debug.LogWarning($"[{GetType().Name}] enemyShoot not assigned on {gameObject.name}.", this);
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (onPlayerDamaged == null) Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
            if (onScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onScoreAdded not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
