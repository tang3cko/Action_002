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
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(5f, 0f), 0f), Is.True);
        }

        [Test]
        public void IsInWaveRing_TargetInsideInnerEdge_ReturnsFalse()
        {
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(3f, 0f), 0f), Is.False);
        }

        [Test]
        public void IsInWaveRing_TargetOutsideOuterEdge_ReturnsFalse()
        {
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(7f, 0f), 0f), Is.False);
        }

        [Test]
        public void IsInWaveRing_LargeTargetRadius_ExtendsRange()
        {
            Assert.That(WaveCollisionCalculator.IsInWaveRing(
                float2.zero, 5f, 1f, new float2(4f, 0f), 1f), Is.True);
        }

        // --- IsHit ---

        [Test]
        public void IsHit_TargetInRing_ReturnsTrue()
        {
            var wave = new WaveState
            {
                Origin = float2.zero,
                CurrentRadius = 5f,
                MaxRadius = 10f,
            };

            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(5f, 0f), 0f), Is.True);
            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(0f, 5f), 0f), Is.True);
        }

        [Test]
        public void IsHit_TargetOutsideRing_ReturnsFalse()
        {
            var wave = new WaveState
            {
                Origin = float2.zero,
                CurrentRadius = 5f,
                MaxRadius = 10f,
            };

            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(1f, 0f), 0f), Is.False);
            Assert.That(WaveCollisionCalculator.IsHit(wave, 1f, new float2(7f, 0f), 0f), Is.False);
        }
    }
}
