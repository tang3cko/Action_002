using UnityEngine;
using Unity.Mathematics;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Tang3cko.ReactiveSO;

namespace Action002.Enemy.Systems
{
    public class EnemySpawnSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;

        private float spawnTimer;
        private float elapsedTime;
        private int nextId = 1;
        private Unity.Mathematics.Random rng;

        private void Start()
        {
            rng = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        }

        public void ProcessSpawning()
        {
            elapsedTime += Time.deltaTime;
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f && enemySet.Count < gameConfig.MaxEnemies)
            {
                SpawnEnemy();
                float interval = SpawnCalculator.GetSpawnInterval(gameConfig.BaseSpawnInterval, elapsedTime, gameConfig.MinSpawnInterval);
                spawnTimer = interval;
            }
        }

        private void SpawnEnemy()
        {
            float angle = rng.NextFloat(0f, math.PI * 2f);
            float2 spawnPos = SpawnCalculator.GetSpawnPosition(new float2(playerPositionVar.Value.x, playerPositionVar.Value.y), gameConfig.SpawnRadius, angle);
            Polarity polarity = SpawnCalculator.GetRandomPolarity(rng.NextFloat());

            float speedVariance = rng.NextFloat(0.8f, 1.2f);

            var state = new EnemyState
            {
                Position = spawnPos,
                Speed = 2f * speedVariance * (1f + elapsedTime * 0.003f),
                Hp = 1,
                Polarity = (byte)polarity,
            };

            int id = nextId++;
            enemySet.Register(id, state);
        }

        public void ResetForNewRun()
        {
            spawnTimer = 0f;
            elapsedTime = 0f;
            nextId = 1;
            rng = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
