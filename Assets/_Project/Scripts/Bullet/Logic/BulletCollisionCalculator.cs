using Unity.Mathematics;

namespace Action002.Bullet.Logic
{
    public static class BulletCollisionCalculator
    {
        public static bool IsPlayerBullet(Data.BulletFaction faction)
        {
            return faction == Data.BulletFaction.Player;
        }

        public static bool IsWithinRadius(float2 posA, float2 posB, float radius)
        {
            return math.distancesq(posA, posB) <= radius * radius;
        }

        public static bool ShouldAbsorb(bool isSamePolarity, float2 bulletPos, float2 playerPos, float absorbRadius)
        {
            return isSamePolarity && IsWithinRadius(bulletPos, playerPos, absorbRadius);
        }

        public static bool ShouldDamagePlayer(bool isSamePolarity, float2 bulletPos, float2 playerPos, float damageRadius)
        {
            return !isSamePolarity && IsWithinRadius(bulletPos, playerPos, damageRadius);
        }

        public static int CalculateRemainingHp(int currentHp, int damage)
        {
            return currentHp - damage;
        }

        public static bool IsEnemyKilled(int remainingHp)
        {
            return remainingHp <= 0;
        }
    }
}
