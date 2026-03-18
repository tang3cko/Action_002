using Unity.Mathematics;

namespace Action002.Bullet.Data
{
    public struct BulletState
    {
        public float2 Position;
        public float2 Velocity;
        public float ScoreValue;
        public byte Polarity;
        public byte Faction;   // 0 = Player, 1 = Enemy
        public int Damage;
    }
}
