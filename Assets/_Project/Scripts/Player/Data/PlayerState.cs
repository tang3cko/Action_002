using Unity.Mathematics;
using Action002.Core;

namespace Action002.Player.Data
{
    public struct PlayerState
    {
        public float2 Position;
        public Polarity CurrentPolarity;
        public int Hp;
        public int MaxHp;
        public float InvincibleTimer;
        public int ComboCount;
        public float ComboTimer;
        public float ComboMultiplier;
        public float SpinGauge;
        public int Score;
    }
}
