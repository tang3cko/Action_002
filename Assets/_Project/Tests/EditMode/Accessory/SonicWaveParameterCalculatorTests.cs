using NUnit.Framework;
using Unity.Mathematics;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class SonicWaveParameterCalculatorTests
    {
        private const float BASE_MAX_RADIUS = 5f;
        private const float BASE_EXPAND_SPEED = 8f;

        [Test]
        public void Calculate_Level1_SmallPulse_ReturnsBaseValuesWithSmallScale()
        {
            var p = SonicWaveParameterCalculator.Calculate(1, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 0.6f).Within(0.001f));
            Assert.That(p.ExpandSpeed, Is.EqualTo(BASE_EXPAND_SPEED));
            Assert.That(p.Damage, Is.EqualTo(1));
            Assert.That(p.ArcHalfSpread, Is.EqualTo(math.PI).Within(0.0001f));
        }

        [Test]
        public void Calculate_Level1_LargePulse_ReturnsLargeRadiusAndSlowSpeed()
        {
            var p = SonicWaveParameterCalculator.Calculate(1, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.LargePulse);

            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 1.2f).Within(0.001f));
            Assert.That(p.ExpandSpeed, Is.EqualTo(BASE_EXPAND_SPEED * 0.5f).Within(0.001f));
            Assert.That(p.Damage, Is.EqualTo(1));
            Assert.That(p.ArcHalfSpread, Is.EqualTo(math.PI).Within(0.0001f));
        }

        [Test]
        public void Calculate_Level2_IncreasesDamage()
        {
            var p = SonicWaveParameterCalculator.Calculate(2, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            Assert.That(p.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Calculate_Level3_IncreasesMaxRadius()
        {
            var p = SonicWaveParameterCalculator.Calculate(3, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            // Level3: baseMaxRadius * 1.4f, then SmallPulse: * 0.6f
            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 1.4f * 0.6f).Within(0.001f));
            Assert.That(p.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Calculate_Level4_IncreasesExpandSpeed()
        {
            var p = SonicWaveParameterCalculator.Calculate(4, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            Assert.That(p.ExpandSpeed, Is.EqualTo(BASE_EXPAND_SPEED * 1.25f).Within(0.001f));
        }

        [Test]
        public void Calculate_Level4_LargePulse_ExpandSpeedCombinesLevelAndBeat()
        {
            var p = SonicWaveParameterCalculator.Calculate(4, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.LargePulse);

            // Level4: baseExpandSpeed * 1.25f, then LargePulse: * 0.5f
            Assert.That(p.ExpandSpeed, Is.EqualTo(BASE_EXPAND_SPEED * 1.25f * 0.5f).Within(0.001f));
        }

        [Test]
        public void Calculate_Level5_IncreasesMaxRadiusAndDamage()
        {
            var p = SonicWaveParameterCalculator.Calculate(5, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            // Level5: baseMaxRadius * 1.8f, then SmallPulse: * 0.6f
            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 1.8f * 0.6f).Within(0.001f));
            Assert.That(p.Damage, Is.EqualTo(3));
        }

        [Test]
        public void Calculate_LevelZero_ClampsToLevel1()
        {
            var p = SonicWaveParameterCalculator.Calculate(0, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            Assert.That(p.Damage, Is.EqualTo(1));
            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 0.6f).Within(0.001f));
        }

        [Test]
        public void Calculate_NegativeLevel_ClampsToLevel1()
        {
            var p = SonicWaveParameterCalculator.Calculate(-5, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            Assert.That(p.Damage, Is.EqualTo(1));
            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 0.6f).Within(0.001f));
        }

        [Test]
        public void Calculate_ReflectsBaseValues()
        {
            float customRadius = 10f;
            float customSpeed = 16f;
            var p = SonicWaveParameterCalculator.Calculate(1, customRadius, customSpeed, SonicWaveBeat.SmallPulse);

            Assert.That(p.MaxRadius, Is.EqualTo(customRadius * 0.6f).Within(0.001f));
            Assert.That(p.ExpandSpeed, Is.EqualTo(customSpeed));
        }
    }
}
