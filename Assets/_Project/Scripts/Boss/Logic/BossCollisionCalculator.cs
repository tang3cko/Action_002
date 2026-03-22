namespace Action002.Boss.Logic
{
    public static class BossCollisionCalculator
    {
        public static bool IsWithinRadius(float bulletX, float bulletY,
            float entityX, float entityY, float combinedRadius)
        {
            float dx = bulletX - entityX;
            float dy = bulletY - entityY;
            return (dx * dx + dy * dy) <= combinedRadius * combinedRadius;
        }

        public static int CalculateRemainingHp(int currentHp, int damage)
        {
            return currentHp - damage;
        }

        public static bool IsEntityKilled(int remainingHp)
        {
            return remainingHp <= 0;
        }
    }
}
