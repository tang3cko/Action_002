using Unity.Mathematics;
using Action002.Core;

namespace Action002.Enemy.Logic
{
    public static class SpawnCalculator
    {
        public static float2 GetSpawnPosition(float2 center, float radius, float angle)
        {
            return center + new float2(math.cos(angle), math.sin(angle)) * radius;
        }

        public static Polarity GetRandomPolarity(float randomValue)
        {
            return randomValue < 0.5f ? Polarity.White : Polarity.Black;
        }

        public static float GetSpawnInterval(float baseInterval, float elapsedTime, float minInterval)
        {
            float interval = baseInterval - elapsedTime * 0.01f;
            return math.max(interval, minInterval);
        }
    }
}
