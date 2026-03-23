using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;

namespace Action002.Accessory.SonicWave.Logic
{
    public static class SonicWaveAttackCalculator
    {
        public const float RIGHT_WAVE_CENTER_ANGLE = 0f;
        public const float LEFT_WAVE_CENTER_ANGLE = math.PI;

        public static WaveState CreateArcWave(
            float2 origin, float centerAngle, float halfSpread,
            float maxRadius, float expandSpeed, byte polarity, int damage)
        {
            return new WaveState
            {
                Origin = origin,
                CurrentRadius = 0f,
                MaxRadius = maxRadius,
                ExpandSpeed = expandSpeed,
                ArcCenterAngle = centerAngle,
                ArcHalfSpread = halfSpread,
                Shape = WaveShape.Arc,
                Polarity = polarity,
                Damage = damage,
            };
        }

        public static WaveState CreatePulse(
            float2 origin, float maxRadius, float expandSpeed,
            byte polarity, int damage)
        {
            return new WaveState
            {
                Origin = origin,
                CurrentRadius = 0f,
                MaxRadius = maxRadius,
                ExpandSpeed = expandSpeed,
                ArcCenterAngle = 0f,
                ArcHalfSpread = math.PI,
                Shape = WaveShape.Circle,
                Polarity = polarity,
                Damage = damage,
            };
        }
    }
}
