using Unity.Mathematics;

namespace Action002.Enemy.Data
{
    public struct EnemyState
    {
        public float2 Position;
        public float2 Velocity;
        public float Speed;
        public int Hp;
        public byte Polarity;
        public int EnemyTypeId;
    }
}
