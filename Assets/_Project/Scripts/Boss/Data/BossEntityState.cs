using Unity.Mathematics;

namespace Action002.Boss.Data
{
    public struct BossEntityState
    {
        public float2 Position;
        public int Hp;
        public int MaxHp;
        public byte Polarity;
        public float CollisionRadius;
        public bool IsActive;
    }
}
