using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Bullet.Systems;
using Action002.Enemy.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Tests.Bullet
{
    public class EnemyShootTests
    {
        private StubRhythmClock stubClock;
        private EnemyStateSetSO enemySet;
        private BulletStateSetSO bulletSet;
        private Vector2VariableSO playerPositionVar;
        private EnemyShoot shooter;

        [SetUp]
        public void SetUp()
        {
            stubClock = new StubRhythmClock();
            enemySet = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            bulletSet = ScriptableObject.CreateInstance<BulletStateSetSO>();
            playerPositionVar = ScriptableObject.CreateInstance<Vector2VariableSO>();
            playerPositionVar.Value = Vector2.zero;

            shooter = new EnemyShoot(
                stubClock,
                enemySet,
                bulletSet,
                playerPositionVar,
                maxBulletsPerOffbeat: 100);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(enemySet);
            Object.DestroyImmediate(bulletSet);
            Object.DestroyImmediate(playerPositionVar);
        }

        // ── ProcessShooting ──

        [Test]
        public void ProcessShooting_OnOffbeat_FiresBullets()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
                Polarity = 0,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProcessShooting_NotOnOffbeat_DoesNotFire()
        {
            stubClock.ShouldFireOffbeatResult = false;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessShooting_RespectsMaxBulletsPerOffbeat()
        {
            var limitedShooter = new EnemyShoot(
                stubClock, enemySet, bulletSet, playerPositionVar,
                maxBulletsPerOffbeat: 2);

            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            for (int i = 1; i <= 5; i++)
            {
                var enemy = new EnemyState
                {
                    Position = new float2(0f, 10f * i),
                    Hp = 1,
                };
                enemySet.Register(i, enemy);
            }

            limitedShooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(2));
        }

        [Test]
        public void ProcessShooting_RespectsCooldown()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            stubClock.CurrentHalfBeatIndex = 3;
            shooter.ProcessShooting(0.5f);

            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProcessShooting_AfterCooldownExpires_FiresAgain()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            stubClock.CurrentHalfBeatIndex = 3;
            shooter.ProcessShooting(2f);

            Assert.That(bulletSet.Count, Is.EqualTo(2));
        }

        [Test]
        public void ProcessShooting_NoEnemies_DoesNotFire()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessShooting_BulletFactionIsEnemy()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
                Polarity = (byte)Action002.Core.Polarity.Black,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Data[0].Faction, Is.EqualTo(BulletFaction.Enemy));
        }

        [Test]
        public void ProcessShooting_BulletPolarityMatchesEnemy()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
                Polarity = (byte)Action002.Core.Polarity.Black,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Data[0].Polarity, Is.EqualTo((byte)Action002.Core.Polarity.Black));
        }

        [Test]
        public void ProcessShooting_BulletDirectionTowardsPlayer()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;
            playerPositionVar.Value = Vector2.zero;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Data[0].Velocity.y, Is.LessThan(0f));
        }

        [Test]
        public void ProcessShooting_BulletScoreValueSet()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            // Shooter's ScoreValue from EnemyTypeTable is 50f
            Assert.That(bulletSet.Data[0].ScoreValue, Is.EqualTo(50f));
        }

        [Test]
        public void ProcessShooting_EnemyAtPlayerPosition_SkipsZeroDistance()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;
            playerPositionVar.Value = Vector2.zero;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 0f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(0), "Should skip shooting when distance to player is near zero");
        }

        // ── ResetForNewRun ──

        [Test]
        public void ResetForNewRun_ClearsCooldownsAndIds()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);
            shooter.ProcessShooting(0f);

            // Mirror production: clear bullet set before resetting run
            bulletSet.Clear();
            shooter.ResetForNewRun();

            stubClock.CurrentHalfBeatIndex = 3;
            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(1));

            // Verify that bullet IDs were reset (should start from 100000 again)
            var entityIds = bulletSet.EntityIds;
            Assert.That(entityIds[0], Is.EqualTo(100000));
        }

        [Test]
        public void ResetForNewRun_AllowsOffbeatToFireAgain()
        {
            stubClock.ShouldFireOffbeatResult = true;
            stubClock.CurrentHalfBeatIndex = 1;

            var enemy = new EnemyState
            {
                Position = new float2(0f, 10f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);
            shooter.ProcessShooting(0f);

            // Mirror production: clear bullet set before resetting run
            bulletSet.Clear();
            shooter.ResetForNewRun();

            stubClock.CurrentHalfBeatIndex = 3;
            shooter.ProcessShooting(0f);

            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        // ── Helpers ──

        private class StubRhythmClock : IRhythmClock
        {
            public int CurrentHalfBeatIndex { get; set; }
            public bool ShouldFireOffbeatResult { get; set; }

            public bool StartClock() { return true; }
            public void StopClock() { }
            public void ProcessClock() { }

            public bool ShouldFireOnDownbeat(ref int lastConsumedIndex) => false;

            public bool ShouldFireOnOffbeat(ref int lastConsumedIndex)
            {
                if (ShouldFireOffbeatResult && CurrentHalfBeatIndex > lastConsumedIndex)
                {
                    lastConsumedIndex = CurrentHalfBeatIndex;
                    return true;
                }
                return false;
            }

            public void ResetForNewRun() { }
        }
    }
}
