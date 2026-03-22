using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Player.Systems
{
    public class PlayerAttack
    {
        private readonly IRhythmClock rhythmClock;
        private readonly GameConfigSO gameConfig;
        private readonly EnemyStateSetSO enemySet;
        private readonly BulletStateSetSO bulletSet;
        private readonly Vector2VariableSO playerPositionVar;
        private readonly IntVariableSO playerPolarityVar;
        private readonly IntVariableSO playerBulletCountVar;
        private readonly FloatVariableSO bulletSpeedMultiplierVar;

        private int lastConsumedHalfBeatIndex = -1;
        private int nextBulletId = 200000;

        public PlayerAttack(
            IRhythmClock rhythmClock,
            GameConfigSO gameConfig,
            EnemyStateSetSO enemySet,
            BulletStateSetSO bulletSet,
            Vector2VariableSO playerPositionVar,
            IntVariableSO playerPolarityVar,
            IntVariableSO playerBulletCountVar,
            FloatVariableSO bulletSpeedMultiplierVar)
        {
            this.rhythmClock = rhythmClock;
            this.gameConfig = gameConfig;
            this.enemySet = enemySet;
            this.bulletSet = bulletSet;
            this.playerPositionVar = playerPositionVar;
            this.playerPolarityVar = playerPolarityVar;
            this.playerBulletCountVar = playerBulletCountVar;
            this.bulletSpeedMultiplierVar = bulletSpeedMultiplierVar;
        }

        public void ProcessAttacks()
        {
            if (rhythmClock == null || gameConfig == null || bulletSet == null) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            if (!rhythmClock.ShouldFireOnDownbeat(ref lastConsumedHalfBeatIndex))
                return;

            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            int bulletCount = (playerBulletCountVar != null) ? math.max(1, playerBulletCountVar.Value) : 1;
            float speedMultiplier = (bulletSpeedMultiplierVar != null) ? bulletSpeedMultiplierVar.Value : 1f;
            float bulletSpeed = gameConfig.PlayerBulletSpeed * speedMultiplier;

            if (bulletCount == 1)
            {
                float2 direction = FindDirectionToNearestEnemy(playerPos);
                RegisterBullet(playerPos, direction, bulletSpeed);
            }
            else
            {
                var directions = FindDirectionsToNearestEnemies(playerPos, bulletCount);
                for (int i = 0; i < bulletCount; i++)
                {
                    RegisterBullet(playerPos, directions[i], bulletSpeed);
                }
            }
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextBulletId = 200000;
        }

        private void RegisterBullet(float2 playerPos, float2 direction, float bulletSpeed)
        {
            var bullet = new BulletState
            {
                Position = playerPos,
                Velocity = direction * bulletSpeed,
                ScoreValue = 0f,
                Polarity = (byte)playerPolarityVar.Value,
                Faction = BulletFaction.Player,
                Damage = 1,
            };

            bulletSet.Register(nextBulletId++, bullet);
        }

        private float2 FindDirectionToNearestEnemy(float2 playerPos)
        {
            if (enemySet == null || enemySet.Count == 0)
                return new float2(0f, 1f);

            var data = enemySet.Data;
            float bestDistSq = float.MaxValue;
            float2 bestDir = new float2(0f, 1f);

            for (int i = 0; i < data.Length; i++)
            {
                float2 diff = data[i].Position - playerPos;
                float distSq = math.lengthsq(diff);
                if (distSq < bestDistSq && distSq > 0.0001f)
                {
                    bestDistSq = distSq;
                    bestDir = math.normalize(diff);
                }
            }

            return bestDir;
        }

        private float2[] FindDirectionsToNearestEnemies(float2 playerPos, int count)
        {
            var directions = new float2[count];

            if (enemySet == null || enemySet.Count == 0)
            {
                for (int i = 0; i < count; i++)
                    directions[i] = new float2(0f, 1f);
                return directions;
            }

            var data = enemySet.Data;
            int enemyCount = data.Length;

            // Sort enemies by distance, exclude zero-distance, pick closest N
            var candidates = new (float distSq, int index)[enemyCount];
            int candidateCount = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                float2 diff = data[i].Position - playerPos;
                float distSq = math.lengthsq(diff);
                if (distSq > 0.0001f)
                {
                    candidates[candidateCount++] = (distSq, i);
                }
            }

            if (candidateCount == 0)
            {
                for (int i = 0; i < count; i++)
                    directions[i] = new float2(0f, 1f);
                return directions;
            }

            Array.Sort(candidates, 0, candidateCount, Comparer<(float distSq, int index)>.Create((a, b) => a.distSq.CompareTo(b.distSq)));

            for (int i = 0; i < count; i++)
            {
                int pickIndex = math.min(i, candidateCount - 1);
                int enemyIndex = candidates[pickIndex].index;
                float2 diff = data[enemyIndex].Position - playerPos;
                directions[i] = math.normalize(diff);
            }

            return directions;
        }
    }
}
