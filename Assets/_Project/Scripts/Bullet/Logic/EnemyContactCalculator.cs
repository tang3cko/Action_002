using Unity.Mathematics;
using Action002.Core;

namespace Action002.Bullet.Logic
{
    public static class EnemyContactCalculator
    {
        public static bool IsContact(float2 playerPos, float2 enemyPos, float contactRadius)
        {
            return math.distancesq(playerPos, enemyPos) <= contactRadius * contactRadius;
        }

        public static bool IsSamePolarity(Polarity player, byte enemyPolarity)
        {
            return (byte)player == enemyPolarity;
        }
    }
}
