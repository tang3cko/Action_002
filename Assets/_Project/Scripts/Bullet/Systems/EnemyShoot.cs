using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Tang3cko.ReactiveSO;

namespace Action002.Bullet.Systems
{
    public class EnemyShoot
    {
        private readonly IRhythmClock rhythmClock;
        private readonly EnemyStateSetSO enemySet;
        private readonly BulletStateSetSO bulletSet;
        private readonly Vector2VariableSO playerPositionVar;
        private readonly int maxBulletsPerOffbeat;

        private int lastConsumedHalfBeatIndex = -1;
        private int nextBulletId = 100000;
        private readonly Dictionary<int, float> lastShotTimes = new Dictionary<int, float>(256);
        private readonly Dictionary<int, float> spiralAngles = new Dictionary<int, float>(64);
        private readonly Dictionary<int, Unity.Mathematics.Random> shotRngs = new Dictionary<int, Unity.Mathematics.Random>(64);

        public EnemyShoot(
            IRhythmClock rhythmClock,
            EnemyStateSetSO enemySet,
            BulletStateSetSO bulletSet,
            Vector2VariableSO playerPositionVar,
            int maxBulletsPerOffbeat = 100)
        {
            this.rhythmClock = rhythmClock;
            this.enemySet = enemySet;
            this.bulletSet = bulletSet;
            this.playerPositionVar = playerPositionVar;
            this.maxBulletsPerOffbeat = maxBulletsPerOffbeat;
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
            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            int fired = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (fired >= maxBulletsPerOffbeat) break;

                int enemyId = entityIds[i];
                var enemy = data[i];
                var spec = EnemyTypeTable.Get(enemy.TypeId);

                if (lastShotTimes.TryGetValue(enemyId, out float lastTime)
                    && currentTime - lastTime < spec.ShootCooldown)
                    continue;

                int remaining = maxBulletsPerOffbeat - fired;
                if (spec.ShotPattern.Count > remaining) continue;

                Span<BulletState> buf = stackalloc BulletState[spec.ShotPattern.Count];
                int written;

                switch (spec.ShotPattern.Kind)
                {
                    case Data.ShotPatternKind.Spiral:
                    {
                        if (!spiralAngles.TryGetValue(enemyId, out float currentAngle))
                            currentAngle = 0f;

                        written = ShotPatternCalculator.CalculateSpiral(buf, spec.ShotPattern, enemy.Position, currentAngle, enemy.Polarity, spec.ScoreValue);
                        spiralAngles[enemyId] = currentAngle + math.radians(spec.ShotPattern.ArcDegrees);
                        break;
                    }
                    case Data.ShotPatternKind.RandomSpread:
                    {
                        if (!shotRngs.TryGetValue(enemyId, out var rng))
                        {
                            rng = new Unity.Mathematics.Random((uint)(enemyId * 7919 + 1));
                        }

                        written = ShotPatternCalculator.CalculateRandomSpread(buf, spec.ShotPattern, enemy.Position, ref rng, enemy.Polarity, spec.ScoreValue);
                        shotRngs[enemyId] = rng;
                        break;
                    }
                    default:
                        written = ShotPatternCalculator.Calculate(buf, spec.ShotPattern, enemy.Position, playerPos, enemy.Polarity, spec.ScoreValue);
                        break;
                }

                if (written == 0) continue;

                for (int j = 0; j < written; j++)
                {
                    bulletSet.Register(nextBulletId++, buf[j]);
                }

                lastShotTimes[enemyId] = currentTime;
                fired += written;
            }
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextBulletId = 100000;
            lastShotTimes.Clear();
            spiralAngles.Clear();
            shotRngs.Clear();
        }
    }
}
