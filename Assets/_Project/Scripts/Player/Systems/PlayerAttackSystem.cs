using UnityEngine;
using Unity.Mathematics;
using Action002.Enemy.Data;
using Action002.Player.Logic;
using Action002.Player.Systems;
using Action002.Core;
using Tang3cko.ReactiveSO;
using System.Collections.Generic;

namespace Action002.Player.Systems
{
    public class PlayerAttackSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Events")]
        [SerializeField] private IntEventChannelSO onEnemyKilled;

        [Header("Settings")]
        [SerializeField] private int killScore = 50;

        private float attackTimer;
        private List<int> despawnQueue = new List<int>(256);

        public void ProcessAttacks()
        {
            if (enemySet.Count == 0) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;
            attackTimer = gameConfig.AttackInterval;

            despawnQueue.Clear();
            if (despawnQueue.Capacity < enemySet.Count)
                despawnQueue.Capacity = enemySet.Count;

            var data = enemySet.Data;
            var ids = enemySet.EntityIds;
            var playerPos = player.Position;
            float range = gameConfig.AttackRange;

            for (int i = 0; i < data.Length; i++)
            {
                if (AttackCalculator.IsInRange(playerPos, data[i].Position, range))
                {
                    despawnQueue.Add(ids[i]);
                    player.AddKillScore(killScore);
                    onEnemyKilled?.RaiseEvent(data[i].Polarity);
                }
            }

            foreach (var id in despawnQueue)
            {
                enemySet.Unregister(id);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (player == null) Debug.LogWarning($"[{GetType().Name}] player not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
