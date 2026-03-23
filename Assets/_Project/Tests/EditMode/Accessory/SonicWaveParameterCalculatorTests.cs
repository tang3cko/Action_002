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

            float expectedRadius = BASE_MAX_RADIUS * 0.6f;
            Assert.That(p.MaxRadius, Is.EqualTo(expectedRadius).Within(0.001f));
            Assert.That(p.Duration, Is.EqualTo(expectedRadius / BASE_EXPAND_SPEED).Within(0.001f));
            Assert.That(p.Damage, Is.EqualTo(1));
            Assert.That(p.ArcHalfSpread, Is.EqualTo(math.PI).Within(0.0001f));
        }

        [Test]
        public void Calculate_Level1_LargePulse_ReturnsLargeRadiusAndLongDuration()
        {
            var p = SonicWaveParameterCalculator.Calculate(1, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.LargePulse);

            float expectedRadius = BASE_MAX_RADIUS * 1.2f;
            float expectedSpeed = BASE_EXPAND_SPEED * 0.5f;
            Assert.That(p.MaxRadius, Is.EqualTo(expectedRadius).Within(0.001f));
            Assert.That(p.Duration, Is.EqualTo(expectedRadius / expectedSpeed).Within(0.001f));
            Assert.That(p.Damage, Is.EqualTo(1));
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

            Assert.That(p.MaxRadius, Is.EqualTo(BASE_MAX_RADIUS * 1.4f * 0.6f).Within(0.001f));
            Assert.That(p.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Calculate_Level4_ShortensDuration()
        {
            var p = SonicWaveParameterCalculator.Calculate(4, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

            // Level4: expandSpeed * 1.25, SmallPulse: radius * 0.6
            float expectedRadius = BASE_MAX_RADIUS * 1.4f * 0.6f;
            float expectedSpeed = BASE_EXPAND_SPEED * 1.25f;
            Assert.That(p.Duration, Is.EqualTo(expectedRadius / expectedSpeed).Within(0.001f));
        }

        [Test]
        public void Calculate_Level4_LargePulse_DurationCombinesLevelAndBeat()
        {
            var p = SonicWaveParameterCalculator.Calculate(4, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.LargePulse);

            // Level4: expandSpeed * 1.25, LargePulse: expandSpeed * 0.5, radius * 1.2
            float expectedRadius = BASE_MAX_RADIUS * 1.4f * 1.2f;
            float expectedSpeed = BASE_EXPAND_SPEED * 1.25f * 0.5f;
            Assert.That(p.Duration, Is.EqualTo(expectedRadius / expectedSpeed).Within(0.001f));
        }

        [Test]
        public void Calculate_Level5_IncreasesMaxRadiusAndDamage()
        {
            var p = SonicWaveParameterCalculator.Calculate(5, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);

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

            float expectedRadius = customRadius * 0.6f;
            Assert.That(p.MaxRadius, Is.EqualTo(expectedRadius).Within(0.001f));
            Assert.That(p.Duration, Is.EqualTo(expectedRadius / customSpeed).Within(0.001f));
        }

        [Test]
        public void Calculate_DurationIsPositive_ForAllBeatTypes()
        {
            var small = SonicWaveParameterCalculator.Calculate(1, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.SmallPulse);
            var large = SonicWaveParameterCalculator.Calculate(1, BASE_MAX_RADIUS, BASE_EXPAND_SPEED, SonicWaveBeat.LargePulse);

            Assert.That(small.Duration, Is.GreaterThan(0f));
            Assert.That(large.Duration, Is.GreaterThan(0f));
            Assert.That(large.Duration, Is.GreaterThan(small.Duration));
        }
    }
}
