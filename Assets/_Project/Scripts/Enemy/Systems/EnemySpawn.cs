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
        private float4 worldBounds; // x=minX, y=minY, z=maxX, w=maxY

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

        public void SetWorldBounds(float4 bounds)
        {
            worldBounds = bounds;
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

            // 同時出現制限チェック: 上限到達時は Chase 型にフォールバック
            if (spec.MaxConcurrent > 0 && CountActiveByType(typeId) >= spec.MaxConcurrent)
            {
                typeId = EnemyTypeId.Shooter;
                spec = EnemyTypeTable.Get(typeId);
            }

            float speedVariance = rng.NextFloat(0.8f, 1.2f);

            var state = new EnemyState
            {
                Position = spawnPos,
                Speed = 2f * speedVariance * spec.SpeedMultiplier * (1f + elapsedTime * 0.003f),
                Hp = spec.Hp,
                Polarity = (byte)polarity,
                TypeId = typeId,
            };

            // Anchor 型は targetPosition を決定
            if (spec.Movement == MovementPattern.Anchor)
            {
                state.TargetPosition = PickAnchorTarget();
            }

            // KeepDistance 型は strafeSign を決定
            if (spec.Movement == MovementPattern.KeepDistance)
            {
                state.StrafeSign = rng.NextFloat() < 0.5f ? (sbyte)1 : (sbyte)-1;
            }

            int id = nextId++;
            enemySet.Register(id, state);
        }

        private int CountActiveByType(EnemyTypeId typeId)
        {
            var data = enemySet.Data;
            int count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].TypeId == typeId)
                    count++;
            }
            return count;
        }

        private float2 PickAnchorTarget()
        {
            // worldBounds の角付近からランダムに選択
            float minX = worldBounds.x;
            float minY = worldBounds.y;
            float maxX = worldBounds.z;
            float maxY = worldBounds.w;

            float marginX = (maxX - minX) * 0.2f;
            float marginY = (maxY - minY) * 0.2f;

            // 4つの角から1つ選択
            int corner = (int)math.floor(rng.NextFloat(0f, 4f));
            corner = math.clamp(corner, 0, 3);

            float2 target;
            switch (corner)
            {
                case 0: // 左上
                    target = new float2(
                        rng.NextFloat(minX, minX + marginX),
                        rng.NextFloat(maxY - marginY, maxY));
                    break;
                case 1: // 右上
                    target = new float2(
                        rng.NextFloat(maxX - marginX, maxX),
                        rng.NextFloat(maxY - marginY, maxY));
                    break;
                case 2: // 左下
                    target = new float2(
                        rng.NextFloat(minX, minX + marginX),
                        rng.NextFloat(minY, minY + marginY));
                    break;
                default: // 右下
                    target = new float2(
                        rng.NextFloat(maxX - marginX, maxX),
                        rng.NextFloat(minY, minY + marginY));
                    break;
            }

            return target;
        }
    }
}
