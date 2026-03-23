using NUnit.Framework;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class WaveBoundsCalculatorTests
    {
        [Test]
        public void IsExpired_RadiusBelowMax_ReturnsFalse()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(4f, 5f), Is.False);
        }

        [Test]
        public void IsExpired_RadiusEqualsMax_ReturnsFalse()
        {
            // Strict greater than, so exactly at max is NOT expired
            Assert.That(WaveBoundsCalculator.IsExpired(5f, 5f), Is.False);
        }

        [Test]
        public void IsExpired_RadiusAboveMax_ReturnsTrue()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(5.01f, 5f), Is.True);
        }

        [Test]
        public void IsExpired_ZeroRadius_ReturnsFalse()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(0f, 5f), Is.False);
        }
    }
}
