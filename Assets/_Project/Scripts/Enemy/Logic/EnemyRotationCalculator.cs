using System;
using Unity.Mathematics;
using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    public static class EnemyRotationCalculator
    {
        public const float N_WAY_DEGREES_PER_SECOND = 45f;
        public const float RING_DEGREES_PER_SECOND = 80f;    // 60° / 0.75s = 3 half-beats at 120BPM
        public const float ANCHOR_DEGREES_PER_SECOND = 90f;  // 90° / 1.0s = 2 beats at 120BPM

        public const float RING_STEP_ANGLE = 60f;   // 360 / 6 panels
        public const float ANCHOR_STEP_ANGLE = 90f;  // 360 / 4 arms

        private const float SNAP_DURATION = 0.15f;

        public static float CalculateAngle(EnemyTypeId typeId, float2 enemyPos, float2 playerPos, float rotationAngle)
        {
            return typeId switch
            {
                EnemyTypeId.Shooter => CalculateShooterAngle(enemyPos, playerPos),
                EnemyTypeId.NWay => rotationAngle,
                EnemyTypeId.Ring => rotationAngle,
                EnemyTypeId.Anchor => rotationAngle,
                EnemyTypeId.Rush => CalculateShooterAngle(enemyPos, playerPos),
                EnemyTypeId.Zoning => CalculateShooterAngle(enemyPos, playerPos),
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
                EnemyTypeId.Rush => 0f,
                EnemyTypeId.Zoning => 0f,
                _ => 0f,
            };
        }

        public static float GetStepAngle(EnemyTypeId typeId)
        {
            return typeId switch
            {
                EnemyTypeId.Ring => RING_STEP_ANGLE,
                EnemyTypeId.Anchor => ANCHOR_STEP_ANGLE,
                EnemyTypeId.Rush => 0f,
                _ => 0f,
            };
        }

        public static float GetHoldRatio(EnemyTypeId typeId)
        {
            float stepAngle = GetStepAngle(typeId);
            if (stepAngle <= 0f) return 0f;
            float rotationSpeed = GetRotationSpeed(typeId);
            if (rotationSpeed <= 0f) return 0f;
            float stepDuration = stepAngle / rotationSpeed;
            return math.saturate(1f - SNAP_DURATION / stepDuration);
        }
    }
}
