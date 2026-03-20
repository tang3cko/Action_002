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
            scoreValue: 50f
        );

        static readonly EnemyTypeSpec NWaySpec = new EnemyTypeSpec(
            hp: 1,
            speedMultiplier: 0.8f,
            visualScale: 1.0f,
            collisionRadius: 0.6f,
            shootCooldown: 1.5f,
            shotPattern: new ShotPatternSpec(ShotPatternKind.NWay, 3, 30f, 2.5f),
            scoreValue: 80f
        );

        static readonly EnemyTypeSpec RingSpec = new EnemyTypeSpec(
            hp: 3,
            speedMultiplier: 0.5f,
            visualScale: 1.4f,
            collisionRadius: 1.0f,
            shootCooldown: 2.0f,
            shotPattern: new ShotPatternSpec(ShotPatternKind.Ring, 8, 0f, 2f),
            scoreValue: 150f
        );

        public static EnemyTypeSpec Get(EnemyTypeId id) => id switch
        {
            EnemyTypeId.Shooter => ShooterSpec,
            EnemyTypeId.NWay => NWaySpec,
            EnemyTypeId.Ring => RingSpec,
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown EnemyTypeId"),
        };
    }
}
