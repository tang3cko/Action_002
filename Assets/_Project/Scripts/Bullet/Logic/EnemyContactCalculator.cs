using Unity.Mathematics;

namespace Action002.Bullet.Logic
{
    public static class EnemyContactCalculator
    {
        public static bool IsContact(float2 playerPos, float2 enemyPos, float playerRadius, float enemyRadius)
        {
            float combined = playerRadius + enemyRadius;
            return math.distancesq(playerPos, enemyPos) <= combined * combined;
        }
    }
}
