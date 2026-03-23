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

            if (elapsedTime < 120f)
            {
                // 90-120s: Shooter 35%, NWay 25%, Ring 25%, Anchor 15%
                if (randomValue < 0.35f) return EnemyTypeId.Shooter;
                if (randomValue < 0.60f) return EnemyTypeId.NWay;
                if (randomValue < 0.85f) return EnemyTypeId.Ring;
                return EnemyTypeId.Anchor;
            }

            if (elapsedTime < 150f)
            {
                // 120-150s: Shooter 25%, NWay 20%, Ring 20%, Anchor 15%, Rush 20%
                if (randomValue < 0.25f) return EnemyTypeId.Shooter;
                if (randomValue < 0.45f) return EnemyTypeId.NWay;
                if (randomValue < 0.65f) return EnemyTypeId.Ring;
                if (randomValue < 0.80f) return EnemyTypeId.Anchor;
                return EnemyTypeId.Rush;
            }

            // 150s~: Shooter 15%, NWay 15%, Ring 15%, Anchor 10%, Rush 20%, Zoning 25%
            if (randomValue < 0.15f) return EnemyTypeId.Shooter;
            if (randomValue < 0.30f) return EnemyTypeId.NWay;
            if (randomValue < 0.45f) return EnemyTypeId.Ring;
            if (randomValue < 0.55f) return EnemyTypeId.Anchor;
            if (randomValue < 0.75f) return EnemyTypeId.Rush;
            return EnemyTypeId.Zoning;
        }
    }
}
