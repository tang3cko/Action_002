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

        public static bool IsInArc(
            float2 waveOrigin, float arcCenterAngle, float arcHalfSpread,
            float2 targetPos, float targetRadius)
        {
            float2 diff = targetPos - waveOrigin;
            float dist = math.length(diff);
            if (dist < 0.0001f) return true;

            float angle = math.atan2(diff.y, diff.x);
            float delta = AngleDelta(angle, arcCenterAngle);
            float angleMargin = math.atan2(targetRadius, dist);
            return math.abs(delta) <= arcHalfSpread + angleMargin;
        }

        public static bool IsHit(WaveState wave, float ringThickness,
            float2 targetPos, float targetRadius)
        {
            if (!IsInWaveRing(wave.Origin, wave.CurrentRadius, ringThickness,
                targetPos, targetRadius))
                return false;

            if (wave.Shape == WaveShape.Circle)
                return true;

            return IsInArc(wave.Origin, wave.ArcCenterAngle,
                wave.ArcHalfSpread, targetPos, targetRadius);
        }

        private static float AngleDelta(float a, float b)
        {
            float d = a - b;
            d = d - 2f * math.PI * math.floor((d + math.PI) / (2f * math.PI));
            return d;
        }
    }
}
