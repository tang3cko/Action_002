using NUnit.Framework;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class AbsorptionCalculatorTests
    {
        [Test]
        public void CalculateComboMultiplier_ZeroCombo_ReturnsOne()
        {
            float result = AbsorptionCalculator.CalculateComboMultiplier(0, 0.1f);
            Assert.That(result, Is.EqualTo(1f));
        }

        [Test]
        public void CalculateComboMultiplier_TenCombo_ReturnsTwo()
        {
            float result = AbsorptionCalculator.CalculateComboMultiplier(10, 0.1f);
            Assert.That(result, Is.EqualTo(2f));
        }

        [Test]
        public void CalculateAbsorbScore_AppliesMultiplier()
        {
            float result = AbsorptionCalculator.CalculateAbsorbScore(10f, 2f);
            Assert.That(result, Is.EqualTo(20f));
        }

        [Test]
        public void IsComboExpired_ZeroTimer_ReturnsTrue()
        {
            Assert.That(AbsorptionCalculator.IsComboExpired(0f), Is.True);
        }

        [Test]
        public void IsComboExpired_PositiveTimer_ReturnsFalse()
        {
            Assert.That(AbsorptionCalculator.IsComboExpired(0.5f), Is.False);
        }
    }
}
