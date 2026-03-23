using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    public static class SpawnWaveCalculator
    {
        public static EnemyTypeId SelectType(float elapsedTime, float randomValue)
        {
            if (elapsedTime < 30f)
            {
                // Shooter only
                return EnemyTypeId.Shooter;
            }

            if (elapsedTime < 60f)
            {
                // Shooter 70%, NWay 30%
                return randomValue < 0.7f ? EnemyTypeId.Shooter : EnemyTypeId.NWay;
            }

            if (elapsedTime < 90f)
            {
                // Shooter 45%, NWay 30%, Ring 25%
                if (randomValue < 0.45f) return EnemyTypeId.Shooter;
                if (randomValue < 0.75f) return EnemyTypeId.NWay;
                return EnemyTypeId.Ring;
            }

            // 90s~: Shooter 35%, NWay 25%, Ring 25%, Anchor 15%
            if (randomValue < 0.35f) return EnemyTypeId.Shooter;
            if (randomValue < 0.60f) return EnemyTypeId.NWay;
            if (randomValue < 0.85f) return EnemyTypeId.Ring;
            return EnemyTypeId.Anchor;
        }
    }
}
