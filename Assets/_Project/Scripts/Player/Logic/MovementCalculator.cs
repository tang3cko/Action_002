using Unity.Mathematics;

namespace Action002.Player.Logic
{
    public static class MovementCalculator
    {
        public static float2 CalculateVelocity(float2 input, float moveSpeed)
        {
            return math.normalizesafe(input) * moveSpeed;
        }

        public static float2 ClampPosition(float2 position, float2 min, float2 max)
        {
            return math.clamp(position, min, max);
        }
    }
}
