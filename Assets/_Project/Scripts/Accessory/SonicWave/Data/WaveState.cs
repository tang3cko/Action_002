using Unity.Mathematics;

namespace Action002.Accessory.SonicWave.Data
{
    public enum WaveShape : byte
    {
        Arc = 0,
        Circle = 1,
    }

    public struct WaveState
    {
        public float2 Origin;
        public float CurrentRadius;
        public float MaxRadius;
        public float ExpandSpeed;
        public float ArcCenterAngle;
        public float ArcHalfSpread;
        public WaveShape Shape;
        public byte Polarity;
        public int Damage;
    }
}
