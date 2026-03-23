using NUnit.Framework;
using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class WaveCollisionCalculatorTests
    {
        // --- IsInWaveRing ---

        [Test]
        public void IsInWaveRing_TargetOnRingCenter_ReturnsTrue()
        {
            // Ring at radius=5, thickness=1 → inner=4.5, outer=5.5
            // Target at distance=5 → inside ring
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(5f, 0f), 0f), Is.True);
        }

        [Test]
        public void IsInWaveRing_TargetInsideInnerEdge_ReturnsFalse()
        {
            // Ring at radius=5, thickness=1 → inner=4.5, outer=5.5
            // Target at distance=3 → inside inner edge
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(3f, 0f), 0f), Is.False);
        }

        [Test]
        public void IsInWaveRing_TargetOutsideOuterEdge_ReturnsFalse()
        {
            // Ring at radius=5, thickness=1 → inner=4.5, outer=5.5
            // Target at distance=7 → outside outer edge
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(7f, 0f), 0f), Is.False);
        }

        [Test]
        public void IsInWaveRing_LargeTargetRadius_ExtendsRange()
        {
            // Ring at radius=5, thickness=1 → base inner=4.5, outer=5.5
            // targetRadius=1 → effective inner=3.5, outer=6.5
            // Target at distance=4 → inside extended ring
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(4f, 0f), 1f), Is.True);
        }

        // --- IsInArc ---

        [Test]
        public void IsInArc_TargetDirectlyInFront_ReturnsTrue()
        {
            // Arc centered at angle=0 (right), halfSpread=PI/4
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, 0f, math.PI / 4f, new float2(5f, 0f), 0f), Is.True);
        }

        [Test]
        public void IsInArc_TargetOutsideArc_ReturnsFalse()
        {
            // Arc centered at angle=0 (right), halfSpread=PI/4 (45 degrees)
            // Target at (0, 5) = 90 degrees
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, 0f, math.PI / 4f, new float2(0f, 5f), 0f), Is.False);
        }

        [Test]
        public void IsInArc_TargetAtArcBoundary_ReturnsTrue()
        {
            // Arc centered at angle=0 (right), halfSpread=PI/4
            // Target at 45 degrees exactly
            float2 target = new float2(5f * math.cos(math.PI / 4f), 5f * math.sin(math.PI / 4f));
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, 0f, math.PI / 4f, target, 0f), Is.True);
        }

        [Test]
        public void IsInArc_TargetAtOrigin_ReturnsTrue()
        {
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, 0f, math.PI / 4f, float2.zero, 0f), Is.True);
        }

        [Test]
        public void IsInArc_TargetRadiusExpandsAngle()
        {
            // Arc centered at angle=0 (right), halfSpread=PI/4
            // Target at angle just outside arc but with large radius
            float angle = math.PI / 4f + 0.05f; // slightly outside
            float2 target = new float2(5f * math.cos(angle), 5f * math.sin(angle));
            // With targetRadius=0 → should be false
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, 0f, math.PI / 4f, target, 0f), Is.False);
            // With targetRadius=1 → angle margin allows hit
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, 0f, math.PI / 4f, target, 1f), Is.True);
        }

        // --- IsHit (integration) ---

        [Test]
        public void IsHit_CircleWave_IgnoresArcCheck()
        {
            var wave = new WaveState
            {
                Origin = float2.zero,
                CurrentRadius = 5f,
                MaxRadius = 10f,
                Shape = WaveShape.Circle,
                ArcHalfSpread = math.PI,
            };

            // Target at (0, 5) = 90 degrees, should hit regardless of angle
            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(0f, 5f), 0f), Is.True);
        }

        [Test]
        public void IsHit_ArcWave_RequiresBothRingAndArc()
        {
            var wave = new WaveState
            {
                Origin = float2.zero,
                CurrentRadius = 5f,
                MaxRadius = 10f,
                Shape = WaveShape.Arc,
                ArcCenterAngle = 0f,
                ArcHalfSpread = math.PI / 4f,
            };

            // In ring and in arc
            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(5f, 0f), 0f), Is.True);
            // In ring but outside arc
            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(0f, 5f), 0f), Is.False);
            // In arc but outside ring
            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(1f, 0f), 0f), Is.False);
        }

        // --- AngleDelta normalization ---

        [Test]
        public void IsInArc_WrapsAroundNegativeAngles()
        {
            // Arc centered at angle=PI (left), halfSpread=PI/4
            // Target at angle=-PI+0.1 (just past -PI, should be close to PI)
            float targetAngle = -math.PI + 0.1f;
            float2 target = new float2(5f * math.cos(targetAngle), 5f * math.sin(targetAngle));
            Assert.That(WaveCollisionCalculator.IsInArc(
                float2.zero, math.PI, math.PI / 4f, target, 0f), Is.True);
        }
    }
}
