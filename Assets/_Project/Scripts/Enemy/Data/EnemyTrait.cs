using System;

namespace Action002.Enemy.Data
{
    [Flags]
    public enum EnemyTrait : byte
    {
        None    = 0,
        Shooter = 1 << 0,
        Elite   = 1 << 1,
    }
}
