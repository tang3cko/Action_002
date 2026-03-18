using Unity.Mathematics;

namespace Action002.Bullet.Logic
{
    /// <summary>
    /// Pure C# static calculator for bullet screen-bounds checks.
    /// </summary>
    public static class BulletBoundsCalculator
    {
        /// <summary>
        /// Returns true when the position is outside the rectangle
        /// defined by (min - margin) to (max + margin).
        /// </summary>
        public static bool IsOutsideBounds(float2 pos, float2 min, float2 max, float margin)
        {
            return pos.x < min.x - margin
                || pos.x > max.x + margin
                || pos.y < min.y - margin
                || pos.y > max.y + margin;
        }
    }
}
