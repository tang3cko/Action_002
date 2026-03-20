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

            // Shooter 50%, NWay 30%, Ring 20%
            if (randomValue < 0.5f) return EnemyTypeId.Shooter;
            if (randomValue < 0.8f) return EnemyTypeId.NWay;
            return EnemyTypeId.Ring;
        }
    }
}
