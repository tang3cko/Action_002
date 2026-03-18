using UnityEngine;
using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Player.Logic;
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;
using System.Collections.Generic;

namespace Action002.Bullet.Systems
{
    public class BulletCollisionSystem : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private BulletStateSetSO bulletSet;
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;
        [SerializeField] private IntEventChannelSO onComboChanged;
        [SerializeField] private IntEventChannelSO onEnemyKilled;

        [Header("Settings")]
        [SerializeField] private float absorbRadius = 1.0f;
        [SerializeField] private float damageRadius = 0.5f;
        [SerializeField] private float bulletHitRadius = 0.5f;
        [SerializeField] private int killScore = 50;

        private List<int> despawnQueue = new List<int>(256);
        private List<int> enemyDespawnQueue = new List<int>(64);
        private HashSet<int> _enemyKillSet = new HashSet<int>();

        public void ProcessCollisions()
        {
            if (bulletSet.Count == 0) return;
            despawnQueue.Clear();
            enemyDespawnQueue.Clear();
            _enemyKillSet.Clear();
            if (despawnQueue.Capacity < bulletSet.Count)
                despawnQueue.Capacity = bulletSet.Count;

            var data = bulletSet.Data;
            var ids = bulletSet.EntityIds;
            var playerPos = player.Position;
            var playerPolarity = player.CurrentPolarity;

            for (int i = 0; i < data.Length; i++)
            {
                var bullet = data[i];

                if (bullet.Faction == 0)
                {
                    // Player bullet vs enemies
                    ProcessPlayerBulletVsEnemies(i, bullet, ids[i]);
                }
                else
                {
                    // Enemy bullet vs player (existing behavior)
                    bool samePolarity = PolarityCalculator.IsSamePolarity(playerPolarity, bullet.Polarity);
                    float distSq = math.distancesq(playerPos, bullet.Position);

                    if (samePolarity && distSq <= absorbRadius * absorbRadius)
                    {
                        // Absorb
                        despawnQueue.Add(ids[i]);
                        player.IncrementCombo(bullet.ScoreValue);
                        onComboChanged?.RaiseEvent(player.State.ComboCount);
                    }
                    else if (!samePolarity && distSq <= damageRadius * damageRadius)
                    {
                        // Damage
                        despawnQueue.Add(ids[i]);
                        player.ApplyDamage();
                        onPlayerDamaged?.RaiseEvent();
                    }
                }
            }

            foreach (var id in despawnQueue)
            {
                bulletSet.Unregister(id);
            }

            foreach (var id in enemyDespawnQueue)
            {
                enemySet.Unregister(id);
            }
        }

        private void ProcessPlayerBulletVsEnemies(int bulletIndex, BulletState bullet, int bulletId)
        {
            if (enemySet == null || enemySet.Count == 0) return;

            var enemyData = enemySet.Data;
            var enemyIds = enemySet.EntityIds;
            float hitRadiusSq = bulletHitRadius * bulletHitRadius;

            for (int j = 0; j < enemyData.Length; j++)
            {
                var enemy = enemyData[j];
                float distSq = math.distancesq(bullet.Position, enemy.Position);

                if (distSq <= hitRadiusSq)
                {
                    int enemyId = enemyIds[j];

                    // Skip if this enemy was already killed this frame
                    if (_enemyKillSet.Contains(enemyId))
                    {
                        despawnQueue.Add(bulletId);
                        break;
                    }

                    despawnQueue.Add(bulletId);

                    enemy.Hp -= bullet.Damage;
                    enemyData[j] = enemy;

                    if (enemy.Hp <= 0)
                    {
                        _enemyKillSet.Add(enemyId);
                        enemyDespawnQueue.Add(enemyId);
                        player.AddKillScore(killScore);
                        onEnemyKilled?.RaiseEvent(enemy.Polarity);
                    }

                    break; // one bullet hits one enemy
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (player == null) Debug.LogWarning($"[{GetType().Name}] player not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
