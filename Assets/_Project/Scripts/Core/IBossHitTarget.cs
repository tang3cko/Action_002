using Unity.Mathematics;

namespace Action002.Core
{
    public interface IBossHitTarget
    {
        bool IsActive { get; }
        int EntityCount { get; }
        bool GetEntityInfo(int index, out float2 position,
            out float collisionRadius, out bool isActive);
        bool TryHitAny(float bulletX, float bulletY, float bulletRadius, int damage);
        bool TryApplyDamageToEntity(int entityIndex, int damage);
    }
}
