using Unity.Mathematics;

namespace Action002.Accessory.SonicWave.Data
{
    public struct WaveState
    {
        public float2 Origin;
        public float CurrentRadius;
        public float MaxRadius;
        public float ElapsedTime;
        public float Duration;
        public byte Polarity;
        public int Damage;
    }
}
