using Unity.Mathematics;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Tang3cko.ReactiveSO;

namespace Action002.Enemy.Systems
{
    public class EnemySpawn
    {
        private readonly GameConfigSO gameConfig;
        private readonly EnemyStateSetSO enemySet;
        private readonly Vector2VariableSO playerPositionVar;

        private float spawnTimer;
        private float elapsedTime;
        private int nextId = 1;
        private Unity.Mathematics.Random rng;
        private bool isActive;

        public EnemySpawn(
            GameConfigSO gameConfig,
            EnemyStateSetSO enemySet,
            Vector2VariableSO playerPositionVar,
            uint rngSeed)
        {
            this.gameConfig = gameConfig;
            this.enemySet = enemySet;
            this.playerPositionVar = playerPositionVar;
            rng = new Unity.Mathematics.Random(rngSeed);
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        public void ProcessSpawning(float deltaTime)
        {
            if (!isActive) return;

            elapsedTime += deltaTime;
            spawnTimer -= deltaTime;

            if (spawnTimer <= 0f && enemySet.Count < gameConfig.MaxEnemies)
            {
                SpawnEnemy();
                float interval = SpawnCalculator.GetSpawnInterval(gameConfig.BaseSpawnInterval, elapsedTime, gameConfig.MinSpawnInterval);
                spawnTimer = interval;
            }
        }

        public void ResetForNewRun(uint newSeed)
        {
            spawnTimer = 0f;
            elapsedTime = 0f;
            nextId = 1;
            rng = new Unity.Mathematics.Random(newSeed);
        }

        private void SpawnEnemy()
        {
            float angle = rng.NextFloat(0f, math.PI * 2f);
            float2 spawnPos = SpawnCalculator.GetSpawnPosition(
                new float2(playerPositionVar.Value.x, playerPositionVar.Value.y),
                gameConfig.SpawnRadius, angle);
            Polarity polarity = SpawnCalculator.GetRandomPolarity(rng.NextFloat());

            var typeId = SpawnWaveCalculator.SelectType(elapsedTime, rng.NextFloat());
            var spec = EnemyTypeTable.Get(typeId);

            float speedVariance = rng.NextFloat(0.8f, 1.2f);

            var state = new EnemyState
            {
                Position = spawnPos,
                Speed = 2f * speedVariance * spec.SpeedMultiplier * (1f + elapsedTime * 0.003f),
                Hp = spec.Hp,
                Polarity = (byte)polarity,
                TypeId = typeId,
            };

            int id = nextId++;
            enemySet.Register(id, state);
        }
    }
}
