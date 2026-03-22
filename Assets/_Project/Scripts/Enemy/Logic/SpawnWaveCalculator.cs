using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    public static class SpawnWaveCalculator
    {
        public static EnemyTypeId SelectType(float elapsedTime, float randomValue)
        {
            if (elapsedTime < 30f)
            {
                return EnemyTypeId.Shooter;
            }

            if (elapsedTime < 60f)
            {
                // Shooter 70%, NWay 30%
                return randomValue < 0.7f ? EnemyTypeId.Shooter : EnemyTypeId.NWay;
            }

            // Shooter 40%, NWay 25%, Ring 20%, Anchor 15%
            if (randomValue < 0.4f) return EnemyTypeId.Shooter;
            if (randomValue < 0.65f) return EnemyTypeId.NWay;
            if (randomValue < 0.85f) return EnemyTypeId.Ring;
            return EnemyTypeId.Anchor;
        }
    }
}
