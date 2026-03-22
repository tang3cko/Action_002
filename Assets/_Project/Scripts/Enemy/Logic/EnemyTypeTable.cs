using System;
using Action002.Bullet.Data;
using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    public static class EnemyTypeTable
    {
        static readonly EnemyTypeSpec ShooterSpec = new EnemyTypeSpec(
            hp: 1,
            speedMultiplier: 1.0f,
            visualScale: 0.8f,
            collisionRadius: 0.5f,
            shootCooldown: 1.0f,
            shotPattern: new ShotPatternSpec(ShotPatternKind.Aimed, 1, 0f, 3f),
            scoreValue: 50f,
            movement: MovementPattern.Chase,
            budgetCost: 1f
        );

        static readonly EnemyTypeSpec NWaySpec = new EnemyTypeSpec(
            hp: 1,
            speedMultiplier: 0.8f,
            visualScale: 1.0f,
            collisionRadius: 0.6f,
            shootCooldown: 1.5f,
            shotPattern: new ShotPatternSpec(ShotPatternKind.Spiral, 3, 15f, 2.5f),
            scoreValue: 80f,
            movement: MovementPattern.KeepDistance,
            keepDistance: 8f,
            budgetCost: 2f
        );

        static readonly EnemyTypeSpec RingSpec = new EnemyTypeSpec(
            hp: 3,
            speedMultiplier: 0.5f,
            visualScale: 1.4f,
            collisionRadius: 1.0f,
            shootCooldown: 2.0f,
            shotPattern: new ShotPatternSpec(ShotPatternKind.Ring, 8, 0f, 2f),
            scoreValue: 150f,
            movement: MovementPattern.Chase,
            budgetCost: 3f
        );

        static readonly EnemyTypeSpec AnchorSpec = new EnemyTypeSpec(
            hp: 15,
            speedMultiplier: 0.6f,
            visualScale: 1.8f,
            collisionRadius: 1.2f,
            shootCooldown: 0.3f,
            shotPattern: new ShotPatternSpec(ShotPatternKind.Spiral, 24, 12f, 2.5f),
            scoreValue: 300f,
            movement: MovementPattern.Anchor,
            arrivalThreshold: 0.5f,
            maxConcurrent: 2,
            budgetCost: 5f
        );

        public static EnemyTypeSpec Get(EnemyTypeId id) => id switch
        {
            EnemyTypeId.Shooter => ShooterSpec,
            EnemyTypeId.NWay => NWaySpec,
            EnemyTypeId.Ring => RingSpec,
            EnemyTypeId.Anchor => AnchorSpec,
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown EnemyTypeId"),
        };
    }
}
