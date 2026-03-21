using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Bullet.Systems;
using Action002.Core;
using Action002.Enemy.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Tests.Bullet
{
    public class BulletCollisionTests
    {
        private BulletStateSetSO bulletSet;
        private EnemyStateSetSO enemySet;
        private Vector2VariableSO playerPositionVar;
        private IntVariableSO playerPolarityVar;
        private VoidEventChannelSO onPlayerDamaged;
        private IntEventChannelSO onEnemyKilled;
        private FloatEventChannelSO onComboIncremented;
        private IntEventChannelSO onKillScoreAdded;
        private BulletCollision collision;

        [SetUp]
        public void SetUp()
        {
            bulletSet = ScriptableObject.CreateInstance<BulletStateSetSO>();
            enemySet = ScriptableObject.CreateInstance<EnemyStateSetSO>();
            playerPositionVar = ScriptableObject.CreateInstance<Vector2VariableSO>();
            playerPolarityVar = ScriptableObject.CreateInstance<IntVariableSO>();
            onPlayerDamaged = ScriptableObject.CreateInstance<VoidEventChannelSO>();
            onEnemyKilled = ScriptableObject.CreateInstance<IntEventChannelSO>();
            onComboIncremented = ScriptableObject.CreateInstance<FloatEventChannelSO>();
            onKillScoreAdded = ScriptableObject.CreateInstance<IntEventChannelSO>();

            playerPositionVar.Value = Vector2.zero;
            playerPolarityVar.Value = (int)Polarity.White;

            collision = new BulletCollision(
                bulletSet,
                enemySet,
                playerPositionVar,
                playerPolarityVar,
                onPlayerDamaged,
                onEnemyKilled,
                onComboIncremented,
                onKillScoreAdded,
                absorbRadius: 1.0f,
                damageRadius: 0.5f,
                bulletHitRadius: 0.5f,
                killScore: 50);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(bulletSet);
            Object.DestroyImmediate(enemySet);
            Object.DestroyImmediate(playerPositionVar);
            Object.DestroyImmediate(playerPolarityVar);
            Object.DestroyImmediate(onPlayerDamaged);
            Object.DestroyImmediate(onEnemyKilled);
            Object.DestroyImmediate(onComboIncremented);
            Object.DestroyImmediate(onKillScoreAdded);
        }

        // ── ProcessCollisions - No bullets ──

        [Test]
        public void ProcessCollisions_NoBullets_DoesNothing()
        {
            Assert.DoesNotThrow(() => collision.ProcessCollisions());
        }

        [Test]
        public void ProcessCollisions_NoBullets_EnemiesUnchanged()
        {
            var enemy = new EnemyState { Position = new float2(0f, 0f), Hp = 1, Polarity = 0 };
            enemySet.Register(1, enemy);

            collision.ProcessCollisions();

            Assert.That(enemySet.Count, Is.EqualTo(1));
        }

        // ── Player bullet vs enemy ──

        [Test]
        public void ProcessCollisions_PlayerBulletHitsEnemy_DespawnsBoth()
        {
            var bullet = new BulletState
            {
                Position = new float2(5f, 5f),
                Velocity = new float2(0f, 1f),
                Faction = BulletFaction.Player,
                Damage = 1,
            };
            bulletSet.Register(200000, bullet);

            var enemy = new EnemyState
            {
                Position = new float2(5f, 5f),
                Hp = 1,
                Polarity = 0,
            };
            enemySet.Register(1, enemy);

            collision.ProcessCollisions();

            Assert.That(bulletSet.Count, Is.EqualTo(0));
            Assert.That(enemySet.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessCollisions_PlayerBulletHitsEnemy_ReducesHpButDoesNotKill()
        {
            int killCount = 0;
            onEnemyKilled.OnEventRaised += (_) => killCount++;

            var bullet = new BulletState
            {
                Position = new float2(5f, 5f),
                Velocity = new float2(0f, 1f),
                Faction = BulletFaction.Player,
                Damage = 1,
            };
            bulletSet.Register(200000, bullet);

            var enemy = new EnemyState
            {
                Position = new float2(5f, 5f),
                Hp = 2,
                Polarity = 0,
            };
            enemySet.Register(1, enemy);

            collision.ProcessCollisions();

            Assert.That(bulletSet.Count, Is.EqualTo(0), "Bullet should be despawned");
            Assert.That(enemySet.Count, Is.EqualTo(1), "Enemy should survive with remaining HP");
            Assert.That(enemySet.Data[0].Hp, Is.EqualTo(1), "Enemy HP should be reduced by bullet damage");
            Assert.That(killCount, Is.EqualTo(0), "Enemy kill event should not fire");
        }

        [Test]
        public void ProcessCollisions_PlayerBulletKillsEnemy_FiresKillEvent()
        {
            int killedPolarity = -1;
            onEnemyKilled.OnEventRaised += (val) => killedPolarity = val;

            var bullet = new BulletState
            {
                Position = new float2(5f, 5f),
                Velocity = new float2(0f, 1f),
                Faction = BulletFaction.Player,
                Damage = 1,
            };
            bulletSet.Register(200000, bullet);

            var enemy = new EnemyState
            {
                Position = new float2(5f, 5f),
                Hp = 1,
                Polarity = (byte)Polarity.Black,
            };
            enemySet.Register(1, enemy);

            collision.ProcessCollisions();

            Assert.That(killedPolarity, Is.EqualTo((int)Polarity.Black));
        }

        [Test]
        public void ProcessCollisions_PlayerBulletKillsEnemy_FiresScoreEvent()
        {
            int scoreAdded = 0;
            onKillScoreAdded.OnEventRaised += (val) => scoreAdded = val;

            var bullet = new BulletState
            {
                Position = new float2(5f, 5f),
                Faction = BulletFaction.Player,
                Damage = 1,
            };
            bulletSet.Register(200000, bullet);

            var enemy = new EnemyState
            {
                Position = new float2(5f, 5f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            collision.ProcessCollisions();

            Assert.That(scoreAdded, Is.EqualTo(50));
        }

        [Test]
        public void ProcessCollisions_PlayerBulletMissesEnemy_NoDespawn()
        {
            var bullet = new BulletState
            {
                Position = new float2(0f, 0f),
                Faction = BulletFaction.Player,
                Damage = 1,
            };
            bulletSet.Register(200000, bullet);

            var enemy = new EnemyState
            {
                Position = new float2(100f, 100f),
                Hp = 1,
            };
            enemySet.Register(1, enemy);

            collision.ProcessCollisions();

            Assert.That(bulletSet.Count, Is.EqualTo(1));
            Assert.That(enemySet.Count, Is.EqualTo(1));
        }

        // ── Enemy bullet vs player ──

        [Test]
        public void ProcessCollisions_EnemyBulletSamePolarity_Absorbs()
        {
            float comboValue = 0f;
            onComboIncremented.OnEventRaised += (val) => comboValue = val;

            playerPolarityVar.Value = (int)Polarity.White;

            var bullet = new BulletState
            {
                Position = new float2(0f, 0f),
                Velocity = new float2(0f, -1f),
                ScoreValue = 10f,
                Polarity = (byte)Polarity.White,
                Faction = BulletFaction.Enemy,
                Damage = 1,
            };
            bulletSet.Register(100000, bullet);

            collision.ProcessCollisions();

            Assert.That(bulletSet.Count, Is.EqualTo(0));
            Assert.That(comboValue, Is.EqualTo(10f));
        }

        [Test]
        public void ProcessCollisions_EnemyBulletDifferentPolarity_DamagesPlayer()
        {
            bool damaged = false;
            onPlayerDamaged.OnEventRaised += () => damaged = true;

            playerPolarityVar.Value = (int)Polarity.White;

            var bullet = new BulletState
            {
                Position = new float2(0f, 0f),
                Polarity = (byte)Polarity.Black,
                Faction = BulletFaction.Enemy,
                Damage = 1,
            };
            bulletSet.Register(100000, bullet);

            collision.ProcessCollisions();

            Assert.That(damaged, Is.True);
            Assert.That(bulletSet.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessCollisions_EnemyBulletOutOfRange_NoEffect()
        {
            bool damaged = false;
            onPlayerDamaged.OnEventRaised += () => damaged = true;
            float comboValue = -1f;
            onComboIncremented.OnEventRaised += (val) => comboValue = val;

            var bullet = new BulletState
            {
                Position = new float2(100f, 100f),
                Polarity = (byte)Polarity.White,
                Faction = BulletFaction.Enemy,
                Damage = 1,
            };
            bulletSet.Register(100000, bullet);

            collision.ProcessCollisions();

            Assert.That(damaged, Is.False);
            Assert.That(comboValue, Is.EqualTo(-1f));
            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProcessCollisions_EnemyBulletSamePolarity_OutOfAbsorbRange_NoAbsorb()
        {
            float comboValue = -1f;
            onComboIncremented.OnEventRaised += (val) => comboValue = val;

            playerPolarityVar.Value = (int)Polarity.White;

            var bullet = new BulletState
            {
                Position = new float2(5f, 5f),
                Polarity = (byte)Polarity.White,
                Faction = BulletFaction.Enemy,
            };
            bulletSet.Register(100000, bullet);

            collision.ProcessCollisions();

            Assert.That(comboValue, Is.EqualTo(-1f));
            Assert.That(bulletSet.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProcessCollisions_MultipleBullets_ProcessesAll()
        {
            int killCount = 0;
            onEnemyKilled.OnEventRaised += (_) => killCount++;

            var enemy1 = new EnemyState { Position = new float2(1f, 0f), Hp = 1 };
            var enemy2 = new EnemyState { Position = new float2(5f, 0f), Hp = 1 };
            enemySet.Register(1, enemy1);
            enemySet.Register(2, enemy2);

            var bullet1 = new BulletState { Position = new float2(1f, 0f), Faction = BulletFaction.Player, Damage = 1 };
            var bullet2 = new BulletState { Position = new float2(5f, 0f), Faction = BulletFaction.Player, Damage = 1 };
            bulletSet.Register(200000, bullet1);
            bulletSet.Register(200001, bullet2);

            collision.ProcessCollisions();

            Assert.That(killCount, Is.EqualTo(2));
        }
    }
}
