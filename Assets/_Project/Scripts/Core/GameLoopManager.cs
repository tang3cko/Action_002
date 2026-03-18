using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Bullet.Systems;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Action002.Enemy.Systems;
using Action002.Audio.Systems;
using Action002.Player.Logic;
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Core
{
    public class GameLoopManager : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;
        [SerializeField] private PlayerAttackSystem playerAttack;
        [SerializeField] private BulletCollisionSystem bulletCollision;
        [SerializeField] private EnemySpawnSystem enemySpawn;
        [SerializeField] private EnemyShootSystem enemyShoot;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;

        private ReactiveEntitySetOrchestrator<EnemyState> enemyOrchestrator;
        private ReactiveEntitySetOrchestrator<BulletState> bulletOrchestrator;
        private bool hasPendingEnemyJob;
        private bool hasPendingBulletJob;
        private List<int> despawnQueue = new List<int>(256);
        private List<int> enemyDespawnQueue = new List<int>(64);
        private List<int> sameContactIds = new List<int>(64);
        private EnemyContactSessionTracker contactTracker = new EnemyContactSessionTracker();

        private void Start()
        {
            enemyOrchestrator = new ReactiveEntitySetOrchestrator<EnemyState>(enemySet);
            bulletOrchestrator = new ReactiveEntitySetOrchestrator<BulletState>(bulletSet);

            if (rhythmClock != null)
                rhythmClock.StartClock();
        }

        private void Update()
        {
            ScheduleEnemyJob();
            ScheduleBulletJob();
        }

        private void LateUpdate()
        {
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

            RemoveOffscreenBullets();

            if (rhythmClock != null)
                rhythmClock.ProcessClock();

            if (playerAttack != null)
                playerAttack.ProcessAttacks();
            if (bulletCollision != null)
                bulletCollision.ProcessCollisions();

            ProcessEnemyContacts();

            if (player != null && player.CheckDeathImmediate())
                return;

            if (enemySpawn != null)
                enemySpawn.ProcessSpawning();
            if (enemyShoot != null)
                enemyShoot.ProcessShooting();
        }

        // --- Enemy Contact (M4.5) ---

        private void ProcessEnemyContacts()
        {
            if (enemySet == null || enemySet.Count == 0 || player == null || gameConfig == null) return;

            sameContactIds.Clear();
            enemyDespawnQueue.Clear();

            var data = enemySet.Data;
            var ids = enemySet.EntityIds;
            var playerPos = player.Position;
            var playerPolarity = player.CurrentPolarity;
            float contactRadius = gameConfig.ContactRadius;

            for (int i = 0; i < data.Length; i++)
            {
                var enemy = data[i];
                if (!EnemyContactCalculator.IsContact(playerPos, enemy.Position, contactRadius))
                    continue;

                if (PolarityCalculator.IsSamePolarity(playerPolarity, enemy.Polarity))
                {
                    sameContactIds.Add(ids[i]);
                }
                else
                {
                    if (!DamageCalculator.IsInvincible(player.State))
                    {
                        player.ApplyDamage();
                        onPlayerDamaged?.RaiseEvent();
                        enemyDespawnQueue.Add(ids[i]);
                    }
                }
            }

            var newContacts = contactTracker.UpdateContacts(sameContactIds);
            foreach (var id in newContacts)
            {
                player.AddScore(gameConfig.ContactScoreBonus);
            }

            foreach (var id in enemyDespawnQueue)
            {
                enemySet.Unregister(id);
            }
        }

        // --- Jobs & Bounds ---

        private void ScheduleEnemyJob()
        {
            if (enemySet.Count == 0) return;

            var src = enemySet.Data;
            var dst = enemyOrchestrator.GetBackBuffer();

            var job = new EnemyMoveJob
            {
                Src = src,
                Dst = dst,
                PlayerPos = player.Position,
                DeltaTime = Time.deltaTime,
            };

            var handle = job.Schedule(enemySet.Count, 64);
            enemyOrchestrator.ScheduleUpdate(handle, enemySet.Count);
            hasPendingEnemyJob = true;
        }

        private void ScheduleBulletJob()
        {
            if (bulletSet.Count == 0) return;

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
            if (bulletSet.Count == 0) return;
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

        private void OnDestroy()
        {
            enemyOrchestrator?.Dispose();
            enemyOrchestrator = null;
            bulletOrchestrator?.Dispose();
            bulletOrchestrator = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (player == null) Debug.LogWarning($"[{GetType().Name}] player not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (playerAttack == null) Debug.LogWarning($"[{GetType().Name}] playerAttack not assigned on {gameObject.name}.", this);
            if (bulletCollision == null) Debug.LogWarning($"[{GetType().Name}] bulletCollision not assigned on {gameObject.name}.", this);
            if (enemySpawn == null) Debug.LogWarning($"[{GetType().Name}] enemySpawn not assigned on {gameObject.name}.", this);
            if (enemyShoot == null) Debug.LogWarning($"[{GetType().Name}] enemyShoot not assigned on {gameObject.name}.", this);
            if (mainCamera == null) Debug.LogWarning($"[{GetType().Name}] mainCamera not assigned on {gameObject.name}.", this);
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (onPlayerDamaged == null) Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
