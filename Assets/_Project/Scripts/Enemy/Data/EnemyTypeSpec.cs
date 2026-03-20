using Action002.Bullet.Data;

namespace Action002.Enemy.Data
{
    public readonly struct EnemyTypeSpec
    {
        public readonly int Hp;
        public readonly float SpeedMultiplier;
        public readonly float VisualScale;
        public readonly float CollisionRadius;
        public readonly float ShootCooldown;
        public readonly ShotPatternSpec ShotPattern;
        public readonly float ScoreValue;

        public EnemyTypeSpec(
            int hp,
            float speedMultiplier,
            float visualScale,
            float collisionRadius,
            float shootCooldown,
            ShotPatternSpec shotPattern,
            float scoreValue)
        {
            Hp = hp;
            SpeedMultiplier = speedMultiplier;
            VisualScale = visualScale;
            CollisionRadius = collisionRadius;
            ShootCooldown = shootCooldown;
            ShotPattern = shotPattern;
            ScoreValue = scoreValue;
        }
    }
}
