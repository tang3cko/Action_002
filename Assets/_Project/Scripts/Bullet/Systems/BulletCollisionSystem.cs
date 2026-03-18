using UnityEngine;
using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Player.Logic;
using Tang3cko.ReactiveSO;
using System.Collections.Generic;

namespace Action002.Bullet.Systems
{
    public class BulletCollisionSystem : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private BulletStateSetSO bulletSet;
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;
        [SerializeField] private IntEventChannelSO onEnemyKilled;
        [SerializeField] private FloatEventChannelSO onComboIncremented;
        [SerializeField] private IntEventChannelSO onKillScoreAdded;

        [Header("Settings")]
        [SerializeField] private float absorbRadius = 1.0f;
        [SerializeField] private float damageRadius = 0.5f;
        [SerializeField] private float bulletHitRadius = 0.5f;
        [SerializeField] private int killScore = 50;

        private List<int> despawnQueue = new List<int>(256);
        private List<int> enemyDespawnQueue = new List<int>(64);
        private HashSet<int> enemyKillSet = new HashSet<int>();

        public void ProcessCollisions()
        {
            if (bulletSet == null || bulletSet.Count == 0) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            despawnQueue.Clear();
            enemyDespawnQueue.Clear();
            enemyKillSet.Clear();
            if (despawnQueue.Capacity < bulletSet.Count)
                despawnQueue.Capacity = bulletSet.Count;

            var data = bulletSet.Data;
            var ids = bulletSet.EntityIds;
            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            var playerPolarity = (Polarity)playerPolarityVar.Value;

            for (int i = 0; i < data.Length; i++)
            {
                var bullet = data[i];

                if (BulletCollisionCalculator.IsPlayerBullet(bullet.Faction))
                {
                    ProcessPlayerBulletVsEnemies(bullet, ids[i]);
                }
                else
                {
                    bool samePolarity = PolarityCalculator.IsSamePolarity(playerPolarity, bullet.Polarity);

                    if (BulletCollisionCalculator.ShouldAbsorb(samePolarity, bullet.Position, playerPos, absorbRadius))
                    {
                        despawnQueue.Add(ids[i]);
                        onComboIncremented?.RaiseEvent(bullet.ScoreValue);
                    }
                    else if (BulletCollisionCalculator.ShouldDamagePlayer(samePolarity, bullet.Position, playerPos, damageRadius))
                    {
                        despawnQueue.Add(ids[i]);
                        onPlayerDamaged?.RaiseEvent();
                    }
                }
            }

            foreach (var id in despawnQueue)
            {
                bulletSet.Unregister(id);
            }

            if (enemySet != null)
            {
                foreach (var id in enemyDespawnQueue)
                {
                    enemySet.Unregister(id);
                }
            }
        }

        private void ProcessPlayerBulletVsEnemies(BulletState bullet, int bulletId)
        {
            if (enemySet == null || enemySet.Count == 0) return;

            var enemyData = enemySet.Data;
            var enemyIds = enemySet.EntityIds;

            for (int j = 0; j < enemyData.Length; j++)
            {
                var enemy = enemyData[j];

                if (BulletCollisionCalculator.IsWithinRadius(bullet.Position, enemy.Position, bulletHitRadius))
                {
                    int enemyId = enemyIds[j];

                    if (enemyKillSet.Contains(enemyId))
                    {
                        despawnQueue.Add(bulletId);
                        break;
                    }

                    despawnQueue.Add(bulletId);

                    int remainingHp = BulletCollisionCalculator.CalculateRemainingHp(enemy.Hp, bullet.Damage);
                    enemy.Hp = remainingHp;
                    enemyData[j] = enemy;

                    if (BulletCollisionCalculator.IsEnemyKilled(remainingHp))
                    {
                        enemyKillSet.Add(enemyId);
                        enemyDespawnQueue.Add(enemyId);
                        onKillScoreAdded?.RaiseEvent(killScore);
                        onEnemyKilled?.RaiseEvent(enemy.Polarity);
                    }

                    break;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
            if (onPlayerDamaged == null) Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
            if (onEnemyKilled == null) Debug.LogWarning($"[{GetType().Name}] onEnemyKilled not assigned on {gameObject.name}.", this);
            if (onComboIncremented == null) Debug.LogWarning($"[{GetType().Name}] onComboIncremented not assigned on {gameObject.name}.", this);
            if (onKillScoreAdded == null) Debug.LogWarning($"[{GetType().Name}] onKillScoreAdded not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
