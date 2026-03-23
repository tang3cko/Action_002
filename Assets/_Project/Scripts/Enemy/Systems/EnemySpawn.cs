using UnityEngine;
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
        private Unity.Mathematics.Random spawnRng;
        private Unity.Mathematics.Random polarityRng;
        private bool isActive;
        private float4 worldBounds; // x=minX, y=minY, z=maxX, w=maxY

        public EnemySpawn(
            GameConfigSO gameConfig,
            EnemyStateSetSO enemySet,
            Vector2VariableSO playerPositionVar,
            uint runSeed)
        {
            this.gameConfig = gameConfig ?? throw new System.ArgumentNullException(nameof(gameConfig));
            this.enemySet = enemySet ?? throw new System.ArgumentNullException(nameof(enemySet));
            this.playerPositionVar = playerPositionVar ?? throw new System.ArgumentNullException(nameof(playerPositionVar));
            spawnRng = new Unity.Mathematics.Random(SeedHelper.DeriveSpawnSeed(runSeed));
            polarityRng = new Unity.Mathematics.Random(SeedHelper.DerivePolaritySeed(runSeed));
        }

        public void SetWorldBounds(float4 bounds)
        {
            worldBounds = new float4(
                math.min(bounds.x, bounds.z),
                math.min(bounds.y, bounds.w),
                math.max(bounds.x, bounds.z),
                math.max(bounds.y, bounds.w));
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

        public void ResetForNewRun(uint runSeed)
        {
            spawnTimer = 0f;
            elapsedTime = 0f;
            nextId = 1;
            spawnRng = new Unity.Mathematics.Random(SeedHelper.DeriveSpawnSeed(runSeed));
            polarityRng = new Unity.Mathematics.Random(SeedHelper.DerivePolaritySeed(runSeed));
        }

        private void SpawnEnemy()
        {
            float angle = spawnRng.NextFloat(0f, math.PI * 2f);
            float2 spawnPos = SpawnCalculator.GetSpawnPosition(
                new float2(playerPositionVar.Value.x, playerPositionVar.Value.y),
                gameConfig.SpawnRadius, angle);
            Polarity polarity = SpawnCalculator.GetRandomPolarity(polarityRng.NextFloat());

            var typeId = SpawnWaveCalculator.SelectType(elapsedTime, spawnRng.NextFloat());
            var spec = EnemyTypeTable.Get(typeId);

            int effectiveMaxConcurrent = (int)(spec.MaxConcurrent * SpawnCalculator.GetOvertimeMultiplier(elapsedTime));
            if (spec.MaxConcurrent > 0 && CountActiveByType(typeId) >= effectiveMaxConcurrent)
            {
                typeId = EnemyTypeId.Shooter;
                spec = EnemyTypeTable.Get(typeId);
            }

            float speedVariance = spawnRng.NextFloat(0.8f, 1.2f);

            var state = new EnemyState
            {
                Position = spawnPos,
                Speed = 2f * speedVariance * spec.SpeedMultiplier * (1f + elapsedTime * 0.003f) * SpawnCalculator.GetOvertimeMultiplier(elapsedTime),
                Hp = spec.Hp,
                Polarity = (byte)polarity,
                TypeId = typeId,
                SpawnTime = Time.time,
            };

            if (spec.Movement == MovementPattern.Anchor)
            {
                state.TargetPosition = PickAnchorTarget();
            }

            if (spec.Movement == MovementPattern.KeepDistance)
            {
                state.StrafeSign = spawnRng.NextFloat() < 0.5f ? (sbyte)1 : (sbyte)-1;
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
            float minX = worldBounds.x;
            float minY = worldBounds.y;
            float maxX = worldBounds.z;
            float maxY = worldBounds.w;

            float marginX = (maxX - minX) * 0.2f;
            float marginY = (maxY - minY) * 0.2f;

            int corner = (int)math.floor(spawnRng.NextFloat(0f, 4f));
            corner = math.clamp(corner, 0, 3);

            float2 target;
            switch (corner)
            {
                case 0: // 左上
                    target = new float2(
                        spawnRng.NextFloat(minX, minX + marginX),
                        spawnRng.NextFloat(maxY - marginY, maxY));
                    break;
                case 1: // 右上
                    target = new float2(
                        spawnRng.NextFloat(maxX - marginX, maxX),
                        spawnRng.NextFloat(maxY - marginY, maxY));
                    break;
                case 2: // 左下
                    target = new float2(
                        spawnRng.NextFloat(minX, minX + marginX),
                        spawnRng.NextFloat(minY, minY + marginY));
                    break;
                default: // 右下
                    target = new float2(
                        spawnRng.NextFloat(maxX - marginX, maxX),
                        spawnRng.NextFloat(minY, minY + marginY));
                    break;
            }

            return target;
        }
    }
}
