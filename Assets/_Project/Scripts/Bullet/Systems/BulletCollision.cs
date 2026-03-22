using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Action002.Player.Logic;
using Tang3cko.ReactiveSO;
using System.Collections.Generic;

namespace Action002.Bullet.Systems
{
    public class BulletCollision
    {
        private readonly BulletStateSetSO bulletSet;
        private readonly EnemyStateSetSO enemySet;
        private readonly Vector2VariableSO playerPositionVar;
        private readonly IntVariableSO playerPolarityVar;
        private readonly VoidEventChannelSO onPlayerDamaged;
        private readonly IntEventChannelSO onEnemyKilled;
        private readonly FloatEventChannelSO onComboIncremented;
        private readonly IntEventChannelSO onKillScoreAdded;
        private readonly float absorbRadius;
        private readonly float damageRadius;
        private readonly float bulletHitRadius;
        private readonly int killScore;
        private IBossHitTarget bossHitTarget;

        private readonly List<int> despawnQueue = new List<int>(256);
        private readonly List<int> enemyDespawnQueue = new List<int>(64);
        private readonly HashSet<int> enemyKillSet = new HashSet<int>();

        public BulletCollision(
            BulletStateSetSO bulletSet,
            EnemyStateSetSO enemySet,
            Vector2VariableSO playerPositionVar,
            IntVariableSO playerPolarityVar,
            VoidEventChannelSO onPlayerDamaged,
            IntEventChannelSO onEnemyKilled,
            FloatEventChannelSO onComboIncremented,
            IntEventChannelSO onKillScoreAdded,
            float absorbRadius = 1.0f,
            float damageRadius = 0.5f,
            float bulletHitRadius = 0.5f,
            int killScore = 50)
        {
            this.bulletSet = bulletSet;
            this.enemySet = enemySet;
            this.playerPositionVar = playerPositionVar;
            this.playerPolarityVar = playerPolarityVar;
            this.onPlayerDamaged = onPlayerDamaged;
            this.onEnemyKilled = onEnemyKilled;
            this.onComboIncremented = onComboIncremented;
            this.onKillScoreAdded = onKillScoreAdded;
            this.absorbRadius = absorbRadius;
            this.damageRadius = damageRadius;
            this.bulletHitRadius = bulletHitRadius;
            this.killScore = killScore;
        }

        public void SetBossHitTarget(IBossHitTarget target)
        {
            bossHitTarget = target;
        }

        public void ProcessCollisions()
        {
            if (bulletSet == null || bulletSet.Count == 0) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            despawnQueue.Clear();
            enemyDespawnQueue.Clear();
            enemyKillSet.Clear();
            if (despawnQueue.Capacity < bulletSet.Count)
                despawnQueue.Capacity = bulletSet.Count;

            var data = bulletSet.Data;
            var ids = bulletSet.EntityIds;
            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            var playerPolarity = (Polarity)playerPolarityVar.Value;

            for (int i = 0; i < data.Length; i++)
            {
                var bullet = data[i];

                if (BulletCollisionCalculator.IsPlayerBullet(bullet.Faction))
                {
                    if (!ProcessPlayerBulletVsBoss(bullet, ids[i]))
                        ProcessPlayerBulletVsEnemies(bullet, ids[i]);
                }
                else
                {
                    bool samePolarity = PolarityCalculator.IsSamePolarity(playerPolarity, bullet.Polarity);

                    if (BulletCollisionCalculator.ShouldAbsorb(samePolarity, bullet.Position, playerPos, absorbRadius))
                    {
                        despawnQueue.Add(ids[i]);
                        onComboIncremented?.RaiseEvent(bullet.ScoreValue);
                    }
                    else if (BulletCollisionCalculator.ShouldDamagePlayer(samePolarity, bullet.Position, playerPos, damageRadius))
                    {
                        despawnQueue.Add(ids[i]);
                        onPlayerDamaged?.RaiseEvent();
                    }
                }
            }

            foreach (var id in despawnQueue)
                bulletSet.Unregister(id);

            if (enemySet != null)
            {
                foreach (var id in enemyDespawnQueue)
                    enemySet.Unregister(id);
            }
        }

        private bool ProcessPlayerBulletVsBoss(BulletState bullet, int bulletId)
        {
            if (bossHitTarget == null || !bossHitTarget.IsActive) return false;

            if (bossHitTarget.TryHitAny(bullet.Position.x, bullet.Position.y, bulletHitRadius, bullet.Damage))
            {
                despawnQueue.Add(bulletId);
                return true;
            }
            return false;
        }

        private void ProcessPlayerBulletVsEnemies(BulletState bullet, int bulletId)
        {
            if (enemySet == null || enemySet.Count == 0) return;

            var enemyData = enemySet.Data;
            var enemyIds = enemySet.EntityIds;

            for (int j = 0; j < enemyData.Length; j++)
            {
                var enemy = enemyData[j];

                float enemyCollisionRadius = EnemyTypeTable.Get(enemy.TypeId).CollisionRadius;
                if (BulletCollisionCalculator.IsWithinRadius(bullet.Position, enemy.Position, bulletHitRadius + enemyCollisionRadius))
                {
                    int enemyId = enemyIds[j];

                    if (enemyKillSet.Contains(enemyId))
                    {
                        despawnQueue.Add(bulletId);
                        break;
                    }

                    despawnQueue.Add(bulletId);

                    int remainingHp = BulletCollisionCalculator.CalculateRemainingHp(enemy.Hp, bullet.Damage);
                    enemy.Hp = remainingHp;
                    // Use SetData API to properly notify change listeners
                    // instead of directly mutating the backing array.
                    enemySet.SetData(enemyId, enemy);

                    if (BulletCollisionCalculator.IsEnemyKilled(remainingHp))
                    {
                        enemyKillSet.Add(enemyId);
                        enemyDespawnQueue.Add(enemyId);
                        onKillScoreAdded?.RaiseEvent(killScore);
                        onEnemyKilled?.RaiseEvent(enemy.Polarity);
                    }

                    break;
                }
            }
        }
    }
}
