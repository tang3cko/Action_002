using System;

namespace Action002.Bullet.Logic
{
    public static class AbsorptionCalculator
    {
        public static float CalculateComboMultiplier(int comboCount, float step)
        {
            return 1f + comboCount * step;
        }

        public static float CalculateAbsorbScore(float bulletValue, float comboMultiplier)
        {
            return bulletValue * comboMultiplier;
        }

        public static bool IsComboExpired(float comboTimer)
        {
            return comboTimer <= 0f;
        }
    }
}
