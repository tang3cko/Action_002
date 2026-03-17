using System;

namespace Action002.Bullet.Data
{
    [Flags]
    public enum BulletTrait : byte
    {
        None  = 0,
        White = 1 << 0,
        Black = 1 << 1,
    }
}
