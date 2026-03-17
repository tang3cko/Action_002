using Unity.Mathematics;

namespace Action002.Player.Logic
{
    public static class AttackCalculator
    {
        public static bool IsInRange(float2 a, float2 b, float radius)
        {
            return math.distancesq(a, b) <= radius * radius;
        }
    }
}
