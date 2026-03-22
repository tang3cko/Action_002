using System;
using Unity.Mathematics;
using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    public static class EnemyRotationCalculator
    {
        private const float N_WAY_DEGREES_PER_SECOND = 45f;
        private const float RING_DEGREES_PER_SECOND = 90f;
        private const float ANCHOR_WOBBLE_SPEED = 2f;
        private const float ANCHOR_WOBBLE_AMPLITUDE = 15f;

        public static float CalculateAngle(EnemyTypeId typeId, float2 enemyPos, float2 playerPos, float time)
        {
            return typeId switch
            {
                EnemyTypeId.Shooter => CalculateShooterAngle(enemyPos, playerPos),
                EnemyTypeId.NWay => CalculateNWayAngle(time),
                EnemyTypeId.Ring => CalculateRingAngle(time),
                EnemyTypeId.Anchor => CalculateAnchorAngle(time),
                _ => throw new ArgumentOutOfRangeException(nameof(typeId), typeId, "Unknown EnemyTypeId"),
            };
        }

        public static float CalculateShooterAngle(float2 enemyPos, float2 playerPos)
        {
            float2 dir = playerPos - enemyPos;
            if (math.lengthsq(dir) < 0.0001f) return 0f;
            return math.degrees(math.atan2(dir.y, dir.x)) - 90f;
        }

        public static float CalculateNWayAngle(float time)
        {
            return time * N_WAY_DEGREES_PER_SECOND;
        }

        public static float CalculateRingAngle(float time)
        {
            return time * RING_DEGREES_PER_SECOND;
        }

        public static float CalculateAnchorAngle(float time)
        {
            return math.sin(time * ANCHOR_WOBBLE_SPEED) * ANCHOR_WOBBLE_AMPLITUDE;
        }
    }
}
