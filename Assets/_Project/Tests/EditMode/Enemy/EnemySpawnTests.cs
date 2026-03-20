using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Enemy.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Tests.Enemy
{
    public class EnemySpawnTests
    {
        private GameConfigSO gameConfig;
        private EnemyStateSetSO enemySet;
        private Vector2VariableSO playerPositionVar;
        private EnemySpawn spawner;

        [SetUp]
        public void SetUp()
        {
            gameConfig = ScriptableObject.CreateInstance<GameConfigSO>();
            SetSerializedField(gameConfig, "maxEnemies", 1000);
            SetSerializedField(gameConfig, "baseSpawnInterval", 0.75f);
            SetSerializedField(gameConfig, "minSpawnInterval", 0.2f);
            SetSerializedField(gameConfig, "spawnRadius", 15f);

            enemySet = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            playerPositionVar = ScriptableObject.CreateInstance<Vector2VariableSO>();

            spawner = new EnemySpawn(gameConfig, enemySet, playerPositionVar, rngSeed: 42);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameConfig);
            Object.DestroyImmediate(enemySet);
            Object.DestroyImmediate(playerPositionVar);
        }

        // ── ProcessSpawning ──

        [Test]
        public void ProcessSpawning_NotActive_DoesNotSpawn()
        {
            spawner.ProcessSpawning(10f);

            Assert.That(enemySet.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessSpawning_ActiveAndTimerExpired_SpawnsEnemy()
        {
            spawner.SetActive(true);

            spawner.ProcessSpawning(1f);

            Assert.That(enemySet.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ProcessSpawning_MaxEnemiesReached_DoesNotSpawn()
        {
            SetSerializedField(gameConfig, "maxEnemies", 1);
            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);
            int countAfterFirst = enemySet.Count;

            spawner.ProcessSpawning(10f);

            Assert.That(enemySet.Count, Is.EqualTo(countAfterFirst));
        }

        [Test]
        public void ProcessSpawning_DeterministicWithSameSeed()
        {
            var spawnerA = new EnemySpawn(gameConfig, enemySet, playerPositionVar, rngSeed: 123);
            spawnerA.SetActive(true);
            spawnerA.ProcessSpawning(1f);
            var posA = enemySet.Data[0].Position;

            var enemySet2 = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            var spawnerB = new EnemySpawn(gameConfig, enemySet2, playerPositionVar, rngSeed: 123);
            spawnerB.SetActive(true);
            spawnerB.ProcessSpawning(1f);
            var posB = enemySet2.Data[0].Position;

            Object.DestroyImmediate(enemySet2);

            Assert.That(posA.x, Is.EqualTo(posB.x));
            Assert.That(posA.y, Is.EqualTo(posB.y));
        }

        [Test]
        public void ProcessSpawning_SpawnsMultipleOverTime()
        {
            spawner.SetActive(true);

            spawner.ProcessSpawning(1f);
            spawner.ProcessSpawning(1f);
            spawner.ProcessSpawning(1f);

            Assert.That(enemySet.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void ProcessSpawning_EnemyHasPositiveHp()
        {
            spawner.SetActive(true);

            spawner.ProcessSpawning(1f);

            Assert.That(enemySet.Data[0].Hp, Is.GreaterThan(0));
        }

        // ── SetActive ──

        [Test]
        public void SetActive_TogglesSpawning()
        {
            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);
            int countAfterActive = enemySet.Count;

            spawner.SetActive(false);
            spawner.ProcessSpawning(10f);

            Assert.That(enemySet.Count, Is.EqualTo(countAfterActive));
        }

        [Test]
        public void SetActive_ReactivateResumesSpawning()
        {
            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);
            spawner.SetActive(false);
            int countAfterDeactivate = enemySet.Count;

            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);

            Assert.That(enemySet.Count, Is.GreaterThan(countAfterDeactivate));
        }

        // ── ResetForNewRun ──

        [Test]
        public void ResetForNewRun_ClearsState()
        {
            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);

            spawner.ResetForNewRun(99);

            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);

            Assert.That(enemySet.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ResetForNewRun_DifferentSeedProducesDifferentResults()
        {
            spawner.SetActive(true);
            spawner.ProcessSpawning(1f);
            var posFirst = enemySet.Data[0].Position;

            var enemySet2 = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            var spawner2 = new EnemySpawn(gameConfig, enemySet2, playerPositionVar, rngSeed: 42);
            spawner2.ResetForNewRun(999);
            spawner2.SetActive(true);
            spawner2.ProcessSpawning(1f);
            var posSecond = enemySet2.Data[0].Position;

            Object.DestroyImmediate(enemySet2);

            bool different = posFirst.x != posSecond.x || posFirst.y != posSecond.y;
            Assert.That(different, Is.True);
        }

        // ── Helpers ──

        private static void SetSerializedField(GameConfigSO obj, string fieldName, int value)
        {
            var so = new SerializedObject(obj);
            so.FindProperty(fieldName).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedField(GameConfigSO obj, string fieldName, float value)
        {
            var so = new SerializedObject(obj);
            so.FindProperty(fieldName).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
