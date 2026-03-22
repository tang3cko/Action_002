using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
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
            spawner.SetWorldBounds(new float4(-10f, -10f, 10f, 10f));
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
            spawnerA.SetWorldBounds(new float4(-10f, -10f, 10f, 10f));
            spawnerA.SetActive(true);
            spawnerA.ProcessSpawning(1f);
            var posA = enemySet.Data[0].Position;

            var enemySet2 = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            var spawnerB = new EnemySpawn(gameConfig, enemySet2, playerPositionVar, rngSeed: 123);
            spawnerB.SetWorldBounds(new float4(-10f, -10f, 10f, 10f));
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
            spawner2.SetWorldBounds(new float4(-10f, -10f, 10f, 10f));
            spawner2.ResetForNewRun(999);
            spawner2.SetActive(true);
            spawner2.ProcessSpawning(1f);
            var posSecond = enemySet2.Data[0].Position;

            Object.DestroyImmediate(enemySet2);

            bool different = posFirst.x != posSecond.x || posFirst.y != posSecond.y;
            Assert.That(different, Is.True);
        }

        // ── 同時出現制限 ──

        [Test]
        public void ProcessSpawning_MaxConcurrent_FallsBackToShooterWhenLimitReached()
        {
            var anchorSpec = EnemyTypeTable.Get(EnemyTypeId.Anchor);
            Assert.That(anchorSpec.MaxConcurrent, Is.EqualTo(2), "Anchor のMaxConcurrentが2であること");

            // Anchor 型を2体手動登録して上限に達する
            enemySet.Register(9001, new EnemyState { TypeId = EnemyTypeId.Anchor, Hp = 5 });
            enemySet.Register(9002, new EnemyState { TypeId = EnemyTypeId.Anchor, Hp = 5 });

            spawner.SetActive(true);
            spawner.SetWorldBounds(new float4(-10f, -10f, 10f, 10f));

            int beforeCount = enemySet.Count;

            // 大量にスポーンさせる
            for (int i = 0; i < 200; i++)
            {
                spawner.ProcessSpawning(0.5f);
            }

            // スポーン機会が無駄にならず、敵が増えていること
            Assert.Greater(enemySet.Count, beforeCount, "Enemies should spawn (fallback to Shooter)");

            // Anchor 型は2体を超えていないこと
            int anchorCount = 0;
            var data = enemySet.Data;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].TypeId == EnemyTypeId.Anchor)
                    anchorCount++;
            }
            Assert.LessOrEqual(anchorCount, anchorSpec.MaxConcurrent, "Anchor count should not exceed MaxConcurrent");
        }

        // ── Anchor 目標位置 ──

        [Test]
        public void ProcessSpawning_AnchorTarget_WithinWorldBoundsCorners()
        {
            float4 bounds = new float4(-10f, -10f, 10f, 10f);
            spawner.SetWorldBounds(bounds);
            spawner.SetActive(true);

            // 多数スポーンして Anchor 型が出たら位置を検証
            for (int i = 0; i < 200; i++)
            {
                spawner.ProcessSpawning(0.5f);
            }

            float marginX = (bounds.z - bounds.x) * 0.2f; // 4.0
            float marginY = (bounds.w - bounds.y) * 0.2f; // 4.0

            var data = enemySet.Data;
            int anchorFoundCount = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].TypeId != EnemyTypeId.Anchor) continue;
                anchorFoundCount++;

                var target = data[i].TargetPosition;

                // bounds 内であること
                Assert.That(target.x, Is.InRange(bounds.x, bounds.z),
                    $"Anchor target X={target.x} should be within bounds");
                Assert.That(target.y, Is.InRange(bounds.y, bounds.w),
                    $"Anchor target Y={target.y} should be within bounds");

                // 角付近であること（端の20%マージン内に収まる）
                bool nearLeftOrRight = target.x <= bounds.x + marginX || target.x >= bounds.z - marginX;
                bool nearTopOrBottom = target.y <= bounds.y + marginY || target.y >= bounds.w - marginY;
                Assert.IsTrue(nearLeftOrRight && nearTopOrBottom,
                    $"Anchor target ({target.x}, {target.y}) should be near a corner of bounds");
            }

            Assert.Greater(anchorFoundCount, 0, "At least one Anchor type should have spawned");
        }

        // ── KeepDistance strafeSign ──

        [Test]
        public void ProcessSpawning_KeepDistance_HasStrafeSign()
        {
            spawner.SetActive(true);

            for (int i = 0; i < 100; i++)
            {
                spawner.ProcessSpawning(1f);
            }

            var data = enemySet.Data;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].TypeId == EnemyTypeId.NWay)
                {
                    Assert.That(data[i].StrafeSign, Is.EqualTo(1).Or.EqualTo(-1),
                        "KeepDistance 型は StrafeSign が +1 or -1 であること");
                }
            }
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
