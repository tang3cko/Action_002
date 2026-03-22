using Action002.Player.Data;

namespace Action002.Player.Logic
{
    public static class PlayerGrowthCalculator
    {
        public static PlayerGrowthState CreateDefault()
        {
            return new PlayerGrowthState
            {
                Level = 0,
                BulletCount = 1,
                MoveSpeedMultiplier = 1f,
                BulletSpeedMultiplier = 1f,
            };
        }

        public static bool ShouldLevelUp(float spinGauge)
        {
            return spinGauge >= 1f;
        }

        private const int MAX_BULLET_COUNT = 8;

        public static PlayerGrowthState ApplyLevelUp(PlayerGrowthState current)
        {
            current.Level++;

            switch (current.Level)
            {
                case 1:
                    current.BulletCount = 2;
                    break;
                case 2:
                    current.MoveSpeedMultiplier += 0.10f;
                    break;
                case 3:
                    current.BulletCount = 3;
                    break;
                case 4:
                    current.BulletSpeedMultiplier += 0.15f;
                    break;
                case 5:
                    current.BulletCount = 4;
                    break;
                case 6:
                    current.BulletCount = 5;
                    break;
                case 7:
                    current.BulletSpeedMultiplier += 0.15f;
                    break;
                case 8:
                    current.BulletCount = 6;
                    break;
                case 9:
                    current.BulletCount = 7;
                    break;
                case 10:
                    current.BulletCount = MAX_BULLET_COUNT;
                    break;
                default:
                    // Level 11+: capped — no further growth
                    current.BulletCount = MAX_BULLET_COUNT;
                    break;
            }

            return current;
        }
    }
}
