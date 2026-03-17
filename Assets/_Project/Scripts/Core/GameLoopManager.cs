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
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Core
{
    /// <summary>
    /// Central simulation manager following the TinyHistory pattern.
    /// Owns all Orchestrators and controls the full frame lifecycle:
    /// Update:      Schedule Jobs
    /// LateUpdate:  CompleteAndApply → structural changes (Register/Unregister)
    /// </summary>
    public class GameLoopManager : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Systems")]
        [SerializeField] private PlayerAttackSystem playerAttack;
        [SerializeField] private BulletCollisionSystem bulletCollision;
        [SerializeField] private EnemySpawnSystem enemySpawn;
        [SerializeField] private EnemyShootSystem enemyShoot;

        private ReactiveEntitySetOrchestrator<EnemyState> enemyOrchestrator;
        private ReactiveEntitySetOrchestrator<BulletState> bulletOrchestrator;
        private bool hasPendingEnemyJob;
        private bool hasPendingBulletJob;
        private List<int> despawnQueue = new List<int>(256);

        private void Start()
        {
            enemyOrchestrator = new ReactiveEntitySetOrchestrator<EnemyState>(enemySet);
            bulletOrchestrator = new ReactiveEntitySetOrchestrator<BulletState>(bulletSet);
        }

        private void Update()
        {
            ScheduleEnemyJob();
            ScheduleBulletJob();
        }

        private void LateUpdate()
        {
            // 1. Complete all jobs
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

            // 2. Structural changes: Unregister (despawn)
            RemoveExpiredBullets();

            if (playerAttack != null)
                playerAttack.ProcessAttacks();
            if (bulletCollision != null)
                bulletCollision.ProcessCollisions();

            // 3. Structural changes: Register (spawn)
            if (enemySpawn != null)
                enemySpawn.ProcessSpawning();
            if (enemyShoot != null)
                enemyShoot.ProcessShooting();
        }

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

        private void RemoveExpiredBullets()
        {
            if (bulletSet.Count == 0) return;
            despawnQueue.Clear();
            if (despawnQueue.Capacity < bulletSet.Count)
                despawnQueue.Capacity = bulletSet.Count;

            var data = bulletSet.Data;
            var ids = bulletSet.EntityIds;

            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (data[i].Lifetime <= 0f)
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
            if (playerAttack == null) Debug.LogWarning($"[{GetType().Name}] playerAttack not assigned on {gameObject.name}.", this);
            if (bulletCollision == null) Debug.LogWarning($"[{GetType().Name}] bulletCollision not assigned on {gameObject.name}.", this);
            if (enemySpawn == null) Debug.LogWarning($"[{GetType().Name}] enemySpawn not assigned on {gameObject.name}.", this);
            if (enemyShoot == null) Debug.LogWarning($"[{GetType().Name}] enemyShoot not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
