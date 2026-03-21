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
        private int totalSpawned;
        private Unity.Mathematics.Random spawnRng;
        private Unity.Mathematics.Random polarityRng;

        // Triangle wave budget state
        private Polarity currentPolarity;
        private float budgetRemaining;
        private const float BudgetMin = 2f;
        private const float BudgetMax = 8f;
        private const float PeriodStart = 20f;
        private const float PeriodEnd = 8f;
        private const float CostShooter = 1f;
        private const float CostNWay = 2f;
        private const float CostRing = 3f;

        private void Start()
        {
            uint ticks = (uint)System.DateTime.Now.Ticks;
            spawnRng = new Unity.Mathematics.Random(ticks == 0 ? 1 : ticks);
            polarityRng = new Unity.Mathematics.Random((ticks ^ 0x9E3779B9) == 0 ? 1 : ticks ^ 0x9E3779B9);
            currentPolarity = polarityRng.NextFloat() < 0.5f ? Polarity.White : Polarity.Black;
            budgetRemaining = GetTriangleBudget(0);
        }

        public void ProcessSpawning()
        {
            if (gameConfig == null || enemySet == null || playerPositionVar == null) return;

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
            float angle = spawnRng.NextFloat(0f, math.PI * 2f);
            float2 spawnPos = SpawnCalculator.GetSpawnPosition(
                new float2(playerPositionVar.Value.x, playerPositionVar.Value.y),
                gameConfig.SpawnRadius, angle);

            var typeId = SpawnWaveCalculator.SelectType(elapsedTime, spawnRng.NextFloat());
            var spec = EnemyTypeTable.Get(typeId);

            // Triangle wave budget: consume cost, switch polarity when exhausted
            float cost = GetTypeCost(typeId);
            budgetRemaining -= cost;

            Polarity polarity = currentPolarity;

            if (budgetRemaining <= 0f)
            {
                currentPolarity = currentPolarity == Polarity.White ? Polarity.Black : Polarity.White;
                float currentBudget = GetTriangleBudget(totalSpawned);
                float minCost = math.min(CostShooter, math.min(CostNWay, CostRing));
                float variance = (math.floor(polarityRng.NextFloat() * 3f) - 1f) * minCost;
                budgetRemaining = math.max(minCost, currentBudget + variance);
            }

            float speedVariance = spawnRng.NextFloat(0.8f, 1.2f);

            var state = new EnemyState
            {
                Position = spawnPos,
                Speed = 2f * speedVariance * spec.SpeedMultiplier * (1f + elapsedTime * 0.003f),
                Hp = spec.Hp,
                Polarity = (byte)polarity,
                TypeId = typeId,
            };

            int id = nextId++;
            totalSpawned++;
            enemySet.Register(id, state);
        }

        private static float GetTypeCost(EnemyTypeId typeId) => typeId switch
        {
            EnemyTypeId.NWay => CostNWay,
            EnemyTypeId.Ring => CostRing,
            _ => CostShooter,
        };

        private float GetTriangleBudget(int index)
        {
            float t = totalSpawned > 0 ? (float)index / 100f : 0f; // normalize over ~100 spawns
            t = math.clamp(t, 0f, 1f);
            float period = PeriodStart + (PeriodEnd - PeriodStart) * t;
            float phase = (index % period) / period;
            float tri = phase < 0.5f ? phase * 2f : 2f - phase * 2f;
            return math.round(BudgetMin + tri * (BudgetMax - BudgetMin));
        }

        public void ResetForNewRun()
        {
            spawnTimer = 0f;
            elapsedTime = 0f;
            nextId = 1;
            totalSpawned = 0;
            uint ticks = (uint)System.DateTime.Now.Ticks;
            spawnRng = new Unity.Mathematics.Random(ticks == 0 ? 1 : ticks);
            polarityRng = new Unity.Mathematics.Random((ticks ^ 0x9E3779B9) == 0 ? 1 : ticks ^ 0x9E3779B9);
            currentPolarity = polarityRng.NextFloat() < 0.5f ? Polarity.White : Polarity.Black;
            budgetRemaining = GetTriangleBudget(0);
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
