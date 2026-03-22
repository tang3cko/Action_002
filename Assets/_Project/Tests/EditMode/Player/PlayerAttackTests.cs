using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Tests.Player
{
    public class PlayerAttackTests
    {
        private StubRhythmClock stubClock;
        private GameConfigSO gameConfig;
        private EnemyStateSetSO enemySet;
        private BulletStateSetSO bulletSet;
        private Vector2VariableSO playerPositionVar;
        private IntVariableSO playerPolarityVar;
        private IntVariableSO playerBulletCountVar;
        private FloatVariableSO bulletSpeedMultiplierVar;
        private PlayerAttack attack;

        [SetUp]
        public void SetUp()
        {
            stubClock = new StubRhythmClock();
            gameConfig = ScriptableObject.CreateInstance<GameConfigSO>();
            SetSerializedField(gameConfig, "playerBulletSpeed", 15f);
            enemySet = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            bulletSet = ScriptableObject.CreateInstance<BulletStateSetSO>();
            playerPositionVar = ScriptableObject.CreateInstance<Vector2VariableSO>();
            playerPolarityVar = ScriptableObject.CreateInstance<IntVariableSO>();
            playerBulletCountVar = ScriptableObject.CreateInstance<IntVariableSO>();
            bulletSpeedMultiplierVar = ScriptableObject.CreateInstance<FloatVariableSO>();

            playerBulletCountVar.Value = 1;
            bulletSpeedMultiplierVar.Value = 1f;

            attack = new PlayerAttack(
                stubClock,
                gameConfig,
                enemySet,
                bulletSet,
                playerPositionVar,
                playerPolarityVar,
                playerBulletCountVar,
                bulletSpeedMultiplierVar);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameConfig);
            Object.DestroyImmediate(enemySet);
            Object.DestroyImmediate(bulletSet);
            Object.DestroyImmediate(playerPositionVar);
            Object.DestroyImmediate(playerPolarityVar);
            Object.DestroyImmediate(playerBulletCountVar);
            Object.DestroyImmediate(bulletSpeedMultiplierVar);
        }

        // ── ProcessAttacks ──

        [Test]
        public void ProcessAttacks_OnDownbeat_RegistersBullet()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;

            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProcessAttacks_NotOnDownbeat_DoesNotRegister()
        {
            stubClock.ShouldFireResult = false;

            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessAttacks_DirectionTowardsNearestEnemy()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerPositionVar.Value = Vector2.zero;

            var enemyState = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemyState);

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletData[0].Velocity.y, Is.GreaterThan(0f));
        }

        [Test]
        public void ProcessAttacks_NoEnemies_FiresForward()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerPositionVar.Value = Vector2.zero;

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletData[0].Velocity.y, Is.GreaterThan(0f));
        }

        [Test]
        public void ProcessAttacks_BulletFactionIsPlayer()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletData[0].Faction, Is.EqualTo(BulletFaction.Player));
        }

        [Test]
        public void ProcessAttacks_BulletPositionMatchesPlayer()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerPositionVar.Value = new Vector2(3f, 4f);

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletData[0].Position.x, Is.EqualTo(3f));
            Assert.That(bulletData[0].Position.y, Is.EqualTo(4f));
        }

        [Test]
        public void ProcessAttacks_BulletPolarityMatchesPlayer()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerPolarityVar.Value = (int)Polarity.Black;

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletData[0].Polarity, Is.EqualTo((byte)Polarity.Black));
        }

        [Test]
        public void ProcessAttacks_MultipleCalls_IncrementsId()
        {
            stubClock.ShouldFireResult = true;

            stubClock.CurrentHalfBeatIndex = 2;
            attack.ProcessAttacks();
            stubClock.CurrentHalfBeatIndex = 4;
            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(2));
        }

        // ── Multiple Bullets ──

        [Test]
        public void ProcessAttacks_BulletCount2_Registers2Bullets()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerBulletCountVar.Value = 2;
            playerPositionVar.Value = Vector2.zero;

            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(2));
        }

        [Test]
        public void ProcessAttacks_BulletCount3_Registers3Bullets()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerBulletCountVar.Value = 3;
            playerPositionVar.Value = Vector2.zero;

            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(3));
        }

        [Test]
        public void ProcessAttacks_BulletCount2_TwoEnemies_TargetsDifferentEnemies()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerBulletCountVar.Value = 2;
            playerPositionVar.Value = Vector2.zero;

            enemySet.Register(1, new EnemyState { Position = new float2(0f, 10f), Hp = 1 });
            enemySet.Register(2, new EnemyState { Position = new float2(10f, 0f), Hp = 1 });

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletSet.Count, Is.EqualTo(2));
            // One should aim up, one should aim right (or vice versa)
            bool hasUpward = bulletData[0].Velocity.y > 5f || bulletData[1].Velocity.y > 5f;
            bool hasRightward = bulletData[0].Velocity.x > 5f || bulletData[1].Velocity.x > 5f;
            Assert.That(hasUpward, Is.True);
            Assert.That(hasRightward, Is.True);
        }

        [Test]
        public void ProcessAttacks_BulletCount2_OneEnemy_DuplicatesTarget()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerBulletCountVar.Value = 2;
            playerPositionVar.Value = Vector2.zero;

            enemySet.Register(1, new EnemyState { Position = new float2(0f, 10f), Hp = 1 });

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletSet.Count, Is.EqualTo(2));
            // Both should aim at the same enemy (upward)
            Assert.That(bulletData[0].Velocity.y, Is.GreaterThan(0f));
            Assert.That(bulletData[1].Velocity.y, Is.GreaterThan(0f));
        }

        [Test]
        public void ProcessAttacks_BulletCount2_NoEnemies_AllFireForward()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            playerBulletCountVar.Value = 2;
            playerPositionVar.Value = Vector2.zero;

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            Assert.That(bulletSet.Count, Is.EqualTo(2));
            Assert.That(bulletData[0].Velocity.y, Is.GreaterThan(0f));
            Assert.That(bulletData[1].Velocity.y, Is.GreaterThan(0f));
        }

        // ── Bullet Speed Multiplier ──

        [Test]
        public void ProcessAttacks_BulletSpeedMultiplier_AppliedToVelocity()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            bulletSpeedMultiplierVar.Value = 2f;
            playerPositionVar.Value = Vector2.zero;

            attack.ProcessAttacks();

            var bulletData = bulletSet.Data;
            // Base speed 15, multiplier 2, direction (0,1) -> velocity.y = 30
            Assert.That(bulletData[0].Velocity.y, Is.EqualTo(30f).Within(0.01f));
        }

        // ── ResetForNewRun ──

        [Test]
        public void ResetForNewRun_ResetsBeatIndexAndBulletId()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            attack.ProcessAttacks();

            // Mirror production: clear bullets before resetting run
            bulletSet.Clear();
            attack.ResetForNewRun();

            stubClock.CurrentHalfBeatIndex = 4;
            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(1));

            // Verify that bullet IDs were reset (should start from 200000 again)
            var entityIds = bulletSet.EntityIds;
            Assert.That(entityIds[0], Is.EqualTo(200000));
        }

        [Test]
        public void ResetForNewRun_AllowsDownbeatToFireAgain()
        {
            stubClock.ShouldFireResult = true;
            stubClock.CurrentHalfBeatIndex = 2;
            attack.ProcessAttacks();

            // Mirror production: clear bullets before resetting run
            bulletSet.Clear();
            attack.ResetForNewRun();

            stubClock.CurrentHalfBeatIndex = 2;
            attack.ProcessAttacks();

            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        // ── Helpers ──

        private static void SetSerializedField(Object obj, string fieldName, float value)
        {
            var so = new SerializedObject(obj);
            so.FindProperty(fieldName).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private class StubRhythmClock : IRhythmClock
        {
            public int CurrentHalfBeatIndex { get; set; }
            public bool ShouldFireResult { get; set; }

            public bool StartClock() { return true; }
            public void StopClock() { }
            public void ProcessClock() { }

            public bool ShouldFireOnDownbeat(ref int lastConsumedIndex)
            {
                if (ShouldFireResult && CurrentHalfBeatIndex > lastConsumedIndex)
                {
                    lastConsumedIndex = CurrentHalfBeatIndex;
                    return true;
                }
                return false;
            }

            public bool ShouldFireOnOffbeat(ref int lastConsumedIndex) => false;
            public void ResetForNewRun() { }
        }
    }
}
