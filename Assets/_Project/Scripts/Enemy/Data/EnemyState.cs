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
        public EnemyTypeId TypeId;

        /// <summary>Anchor 型の目標位置（Chase/KeepDistance では未使用）</summary>
        public float2 TargetPosition;
        /// <summary>KeepDistance 型の横移動方向（+1 or -1、スポーン時に決定）</summary>
        public sbyte StrafeSign;
        public float SpawnTime;
    }
}
