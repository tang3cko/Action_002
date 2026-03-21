using Unity.Mathematics;

namespace Action002.Bullet.Data
{
    public struct BulletState
    {
        public float2 Position;
        public float2 Velocity;
        public float ScoreValue;
        public byte Polarity;
        public BulletFaction Faction;
        public int Damage;
    }
}
