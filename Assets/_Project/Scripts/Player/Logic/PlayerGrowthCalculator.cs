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
                default:
                    // Level 6+: bullet count capped at 4, move speed +5%
                    current.BulletCount = 4;
                    current.MoveSpeedMultiplier += 0.05f;
                    break;
            }

            return current;
        }
    }
}
