namespace Action002.Enemy.Logic
{
    public static class EnemyDamageCalculator
    {
        public struct DamageResult
        {
            public int RemainingHp;
            public bool IsKilled;
        }

        public static DamageResult ApplyDamage(int currentHp, int damage)
        {
            int remaining = currentHp - damage;
            return new DamageResult
            {
                RemainingHp = remaining,
                IsKilled = remaining <= 0,
            };
        }
    }
}
