using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;

namespace Action002.Accessory.SonicWave.Logic
{
    public static class SonicWaveAttackCalculator
    {
        public static WaveState CreatePulse(
            float2 origin, float maxRadius, float duration,
            byte polarity, int damage)
        {
            return new WaveState
            {
                Origin = origin,
                CurrentRadius = 0f,
                MaxRadius = maxRadius,
                ElapsedTime = 0f,
                Duration = duration,
                Polarity = polarity,
                Damage = damage,
            };
        }
    }
}
