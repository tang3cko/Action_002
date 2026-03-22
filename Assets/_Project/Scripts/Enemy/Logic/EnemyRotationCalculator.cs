using System;
using Unity.Mathematics;
using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    public static class EnemyRotationCalculator
    {
        public const float N_WAY_DEGREES_PER_SECOND = 45f;
        public const float RING_DEGREES_PER_SECOND = 90f;
        public const float ANCHOR_DEGREES_PER_SECOND = 30f;

        public static float CalculateAngle(EnemyTypeId typeId, float2 enemyPos, float2 playerPos, float rotationAngle)
        {
            return typeId switch
            {
                EnemyTypeId.Shooter => CalculateShooterAngle(enemyPos, playerPos),
                EnemyTypeId.NWay => rotationAngle,
                EnemyTypeId.Ring => rotationAngle,
                EnemyTypeId.Anchor => rotationAngle,
                _ => throw new ArgumentOutOfRangeException(nameof(typeId), typeId, "Unknown EnemyTypeId"),
            };
        }

        public static float CalculateShooterAngle(float2 enemyPos, float2 playerPos)
        {
            float2 dir = playerPos - enemyPos;
            if (math.lengthsq(dir) < 0.0001f) return 0f;
            return math.degrees(math.atan2(dir.y, dir.x)) + 90f;
        }

        public static float GetRotationSpeed(EnemyTypeId typeId)
        {
            return typeId switch
            {
                EnemyTypeId.Shooter => 0f,
                EnemyTypeId.NWay => N_WAY_DEGREES_PER_SECOND,
                EnemyTypeId.Ring => RING_DEGREES_PER_SECOND,
                EnemyTypeId.Anchor => ANCHOR_DEGREES_PER_SECOND,
                _ => 0f,
            };
        }
    }
}
