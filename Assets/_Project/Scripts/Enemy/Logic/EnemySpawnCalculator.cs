using Unity.Mathematics;

namespace Action002.Enemy.Logic
{
    public static class EnemySpawnCalculator
    {
        private const float DURATION = 0.2f;

        public static float CalculateScale(float elapsedTime, float baseScale)
        {
            float t = math.saturate(elapsedTime / DURATION);
            t = t - 1f;
            float eased = t * t * ((1.70158f + 1f) * t + 1.70158f) + 1f;
            return baseScale * eased;
        }

        public static bool IsComplete(float elapsedTime)
        {
            return elapsedTime >= DURATION;
        }
    }
}
