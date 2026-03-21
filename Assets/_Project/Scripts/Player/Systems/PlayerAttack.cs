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

        private int lastConsumedHalfBeatIndex = -1;
        private int nextBulletId = 200000;

        public PlayerAttack(
            IRhythmClock rhythmClock,
            GameConfigSO gameConfig,
            EnemyStateSetSO enemySet,
            BulletStateSetSO bulletSet,
            Vector2VariableSO playerPositionVar,
            IntVariableSO playerPolarityVar)
        {
            this.rhythmClock = rhythmClock;
            this.gameConfig = gameConfig;
            this.enemySet = enemySet;
            this.bulletSet = bulletSet;
            this.playerPositionVar = playerPositionVar;
            this.playerPolarityVar = playerPolarityVar;
        }

        public void ProcessAttacks()
        {
            if (rhythmClock == null || gameConfig == null || bulletSet == null) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            if (!rhythmClock.ShouldFireOnDownbeat(ref lastConsumedHalfBeatIndex))
                return;

            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            float2 direction = FindDirectionToNearestEnemy(playerPos);

            var bullet = new BulletState
            {
                Position = playerPos,
                Velocity = direction * gameConfig.PlayerBulletSpeed,
                ScoreValue = 0f,
                Polarity = (byte)playerPolarityVar.Value,
                Faction = BulletFaction.Player,
                Damage = 1,
            };

            bulletSet.Register(nextBulletId++, bullet);
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextBulletId = 200000;
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
    }
}
