using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;

namespace Action002.Accessory.SonicWave.Logic
{
    public static class WaveCollisionCalculator
    {
        public static bool IsInWaveRing(
            float2 waveOrigin, float currentRadius, float ringThickness,
            float2 targetPos, float targetRadius)
        {
            float dist = math.distance(waveOrigin, targetPos);
            float innerEdge = currentRadius - ringThickness * 0.5f - targetRadius;
            float outerEdge = currentRadius + ringThickness * 0.5f + targetRadius;
            return dist >= math.max(0f, innerEdge) && dist <= outerEdge;
        }

        public static bool IsHit(WaveState wave, float ringThickness,
            float2 targetPos, float targetRadius)
        {
            return IsInWaveRing(wave.Origin, wave.CurrentRadius, ringThickness,
                targetPos, targetRadius);
        }
    }
}
