using NUnit.Framework;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class WaveBoundsCalculatorTests
    {
        [Test]
        public void IsExpired_ElapsedBelowDuration_ReturnsFalse()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(0.3f, 0.5f), Is.False);
        }

        [Test]
        public void IsExpired_ElapsedEqualsDuration_ReturnsTrue()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(0.5f, 0.5f), Is.True);
        }

        [Test]
        public void IsExpired_ElapsedAboveDuration_ReturnsTrue()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(0.6f, 0.5f), Is.True);
        }

        [Test]
        public void IsExpired_ZeroElapsed_ReturnsFalse()
        {
            Assert.That(WaveBoundsCalculator.IsExpired(0f, 0.5f), Is.False);
        }
    }
}
