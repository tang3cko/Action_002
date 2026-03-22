using UnityEngine;

namespace Action002.Enemy.Logic
{
    public static class EnemyDeathCalculator
    {
        public const float DURATION = 0.3f;

        public static float CalculateScale(float elapsedTime, float baseScale)
        {
            float t = Mathf.Clamp01(elapsedTime / DURATION);
            if (t >= 1f) return 0f;

            float eased = t * t;
            return baseScale * (1f - eased);
        }

        public static bool IsComplete(float elapsedTime)
        {
            return elapsedTime >= DURATION;
        }
    }
}
