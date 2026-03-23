using NUnit.Framework;
using Unity.Mathematics;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class SonicWaveAttackCalculatorTests
    {
        [Test]
        public void CreatePulse_SetsCorrectFields()
        {
            float2 origin = new float2(3f, 4f);
            var wave = SonicWaveAttackCalculator.CreatePulse(
                origin, 10f, 1.5f, 1, 3);

            Assert.That(wave.Origin.x, Is.EqualTo(3f));
            Assert.That(wave.Origin.y, Is.EqualTo(4f));
            Assert.That(wave.CurrentRadius, Is.EqualTo(0f));
            Assert.That(wave.MaxRadius, Is.EqualTo(10f));
            Assert.That(wave.ElapsedTime, Is.EqualTo(0f));
            Assert.That(wave.Duration, Is.EqualTo(1.5f));
            Assert.That(wave.Polarity, Is.EqualTo(1));
            Assert.That(wave.Damage, Is.EqualTo(3));
        }
    }
}
