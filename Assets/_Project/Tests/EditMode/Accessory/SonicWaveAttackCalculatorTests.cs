using NUnit.Framework;
using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class SonicWaveAttackCalculatorTests
    {
        [Test]
        public void CreateArcWave_SetsCorrectFields()
        {
            float2 origin = new float2(1f, 2f);
            var wave = SonicWaveAttackCalculator.CreateArcWave(
                origin, 0f, math.PI / 4f, 5f, 8f, 0, 2);

            Assert.That(wave.Origin.x, Is.EqualTo(1f));
            Assert.That(wave.Origin.y, Is.EqualTo(2f));
            Assert.That(wave.CurrentRadius, Is.EqualTo(0f));
            Assert.That(wave.MaxRadius, Is.EqualTo(5f));
            Assert.That(wave.ExpandSpeed, Is.EqualTo(8f));
            Assert.That(wave.ArcCenterAngle, Is.EqualTo(0f));
            Assert.That(wave.ArcHalfSpread, Is.EqualTo(math.PI / 4f));
            Assert.That(wave.Shape, Is.EqualTo(WaveShape.Arc));
            Assert.That(wave.Polarity, Is.EqualTo(0));
            Assert.That(wave.Damage, Is.EqualTo(2));
        }

        [Test]
        public void CreatePulse_SetsCorrectFields()
        {
            float2 origin = new float2(3f, 4f);
            var wave = SonicWaveAttackCalculator.CreatePulse(
                origin, 10f, 6f, 1, 3);

            Assert.That(wave.Origin.x, Is.EqualTo(3f));
            Assert.That(wave.Origin.y, Is.EqualTo(4f));
            Assert.That(wave.CurrentRadius, Is.EqualTo(0f));
            Assert.That(wave.MaxRadius, Is.EqualTo(10f));
            Assert.That(wave.ExpandSpeed, Is.EqualTo(6f));
            Assert.That(wave.ArcHalfSpread, Is.EqualTo(math.PI));
            Assert.That(wave.Shape, Is.EqualTo(WaveShape.Circle));
            Assert.That(wave.Polarity, Is.EqualTo(1));
            Assert.That(wave.Damage, Is.EqualTo(3));
        }

        [Test]
        public void CreateArcWave_LeftWave_UsesCorrectCenterAngle()
        {
            var wave = SonicWaveAttackCalculator.CreateArcWave(
                float2.zero, SonicWaveAttackCalculator.LEFT_WAVE_CENTER_ANGLE,
                math.PI / 4f, 5f, 8f, 0, 1);

            Assert.That(wave.ArcCenterAngle, Is.EqualTo(math.PI).Within(0.0001f));
        }

        [Test]
        public void CreateArcWave_RightWave_UsesCorrectCenterAngle()
        {
            var wave = SonicWaveAttackCalculator.CreateArcWave(
                float2.zero, SonicWaveAttackCalculator.RIGHT_WAVE_CENTER_ANGLE,
                math.PI / 4f, 5f, 8f, 0, 1);

            Assert.That(wave.ArcCenterAngle, Is.EqualTo(0f));
        }
    }
}
