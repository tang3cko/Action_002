using Unity.Mathematics;

namespace Action002.Enemy.Data
{
    public struct EnemyDeathParticle
    {
        public float2 Position;
        public byte Polarity;
        public EnemyTypeId TypeId;
        public float ElapsedTime;
    }
}
