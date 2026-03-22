using NUnit.Framework;
using Action002.Boss.Logic;

namespace Action002.Tests.Boss
{
    public class BossCollisionCalculatorTests
    {
        [Test]
        public void IsWithinRadius_Inside_ReturnsTrue()
        {
            Assert.That(BossCollisionCalculator.IsWithinRadius(1f, 0f, 1.5f, 0f, 1f), Is.True);
        }

        [Test]
        public void IsWithinRadius_OnBoundary_ReturnsTrue()
        {
            Assert.That(BossCollisionCalculator.IsWithinRadius(0f, 0f, 1f, 0f, 1f), Is.True);
        }

        [Test]
        public void IsWithinRadius_Outside_ReturnsFalse()
        {
            Assert.That(BossCollisionCalculator.IsWithinRadius(0f, 0f, 3f, 0f, 1f), Is.False);
        }

        [Test]
        public void IsWithinRadius_SamePosition_ReturnsTrue()
        {
            Assert.That(BossCollisionCalculator.IsWithinRadius(5f, 5f, 5f, 5f, 0.1f), Is.True);
        }

        [Test]
        public void CalculateRemainingHp_SubtractsDamage()
        {
            Assert.That(BossCollisionCalculator.CalculateRemainingHp(30, 5), Is.EqualTo(25));
        }

        [Test]
        public void IsEntityKilled_Positive_ReturnsFalse()
        {
            Assert.That(BossCollisionCalculator.IsEntityKilled(1), Is.False);
        }

        [Test]
        public void IsEntityKilled_Zero_ReturnsTrue()
        {
            Assert.That(BossCollisionCalculator.IsEntityKilled(0), Is.True);
        }

        [Test]
        public void IsEntityKilled_Negative_ReturnsTrue()
        {
            Assert.That(BossCollisionCalculator.IsEntityKilled(-5), Is.True);
        }
    }
}
