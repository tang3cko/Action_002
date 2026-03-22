namespace Action002.Core
{
    public interface IBossHitTarget
    {
        bool IsActive { get; }
        bool TryHitAny(float bulletX, float bulletY, float bulletRadius, int damage);
    }
}
