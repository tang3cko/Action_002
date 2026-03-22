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
        public readonly MovementPattern Movement;
        public readonly float KeepDistance;
        public readonly float ArrivalThreshold;
        public readonly int MaxConcurrent;
        public readonly float BudgetCost;

        public EnemyTypeSpec(
            int hp,
            float speedMultiplier,
            float visualScale,
            float collisionRadius,
            float shootCooldown,
            ShotPatternSpec shotPattern,
            float scoreValue,
            MovementPattern movement = MovementPattern.Chase,
            float keepDistance = 0f,
            float arrivalThreshold = 0f,
            int maxConcurrent = 0,
            float budgetCost = 1f)
        {
            Hp = hp;
            SpeedMultiplier = speedMultiplier;
            VisualScale = visualScale;
            CollisionRadius = collisionRadius;
            ShootCooldown = shootCooldown;
            ShotPattern = shotPattern;
            ScoreValue = scoreValue;
            Movement = movement;
            KeepDistance = keepDistance;
            ArrivalThreshold = arrivalThreshold;
            MaxConcurrent = maxConcurrent;
            BudgetCost = budgetCost;
        }
    }
}
