using System.Collections.Generic;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Bullet.Systems
{
    public class EnemyShoot
    {
        private readonly IRhythmClock rhythmClock;
        private readonly EnemyStateSetSO enemySet;
        private readonly BulletStateSetSO bulletSet;
        private readonly Vector2VariableSO playerPositionVar;
        private readonly float bulletSpeed;
        private readonly float bulletScoreValue;
        private readonly int maxBulletsPerOffbeat;
        private readonly float enemyShootCooldown;

        private int lastConsumedHalfBeatIndex = -1;
        private int nextBulletId = 100000;
        private readonly Dictionary<int, float> lastShotTimes = new Dictionary<int, float>(256);

        public EnemyShoot(
            IRhythmClock rhythmClock,
            EnemyStateSetSO enemySet,
            BulletStateSetSO bulletSet,
            Vector2VariableSO playerPositionVar,
            float bulletSpeed = 3f,
            float bulletScoreValue = 10f,
            int maxBulletsPerOffbeat = 100,
            float enemyShootCooldown = 1f)
        {
            this.rhythmClock = rhythmClock;
            this.enemySet = enemySet;
            this.bulletSet = bulletSet;
            this.playerPositionVar = playerPositionVar;
            this.bulletSpeed = bulletSpeed;
            this.bulletScoreValue = bulletScoreValue;
            this.maxBulletsPerOffbeat = maxBulletsPerOffbeat;
            this.enemyShootCooldown = enemyShootCooldown;
        }

        public void ProcessShooting(float currentTime)
        {
            if (enemySet == null || bulletSet == null || playerPositionVar == null) return;
            if (rhythmClock == null) return;
            if (enemySet.Count == 0) return;

            if (!rhythmClock.ShouldFireOnOffbeat(ref lastConsumedHalfBeatIndex))
                return;

            var data = enemySet.Data;
            var entityIds = enemySet.EntityIds;
            int fired = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (fired >= maxBulletsPerOffbeat) break;

                int enemyId = entityIds[i];

                if (lastShotTimes.TryGetValue(enemyId, out float lastTime)
                    && currentTime - lastTime < enemyShootCooldown)
                    continue;

                var enemy = data[i];
                float2 dir = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y) - enemy.Position;
                float dist = math.length(dir);
                if (dist < 0.01f) continue;

                float2 velocity = (dir / dist) * bulletSpeed;

                var bulletState = new BulletState
                {
                    Position = enemy.Position,
                    Velocity = velocity,
                    ScoreValue = bulletScoreValue,
                    Polarity = enemy.Polarity,
                    Faction = 1,
                    Damage = 1,
                };

                bulletSet.Register(nextBulletId++, bulletState);
                lastShotTimes[enemyId] = currentTime;
                fired++;
            }
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextBulletId = 100000;
            lastShotTimes.Clear();
        }
    }
}
