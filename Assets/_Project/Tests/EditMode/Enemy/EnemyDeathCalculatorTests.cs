using NUnit.Framework;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class EnemyDeathCalculatorTests
    {
        [Test]
        public void CalculateScale_AtStart_ReturnsBaseScale()
        {
            float scale = EnemyDeathCalculator.CalculateScale(0f, 1.5f);

            Assert.That(scale, Is.EqualTo(1.5f));
        }

        [Test]
        public void CalculateScale_AfterDuration_ReturnsZero()
        {
            float scale = EnemyDeathCalculator.CalculateScale(EnemyDeathCalculator.DURATION, 1.5f);

            Assert.That(scale, Is.EqualTo(0f));
        }

        [Test]
        public void IsComplete_BeforeDuration_ReturnsFalse()
        {
            bool isComplete = EnemyDeathCalculator.IsComplete(EnemyDeathCalculator.DURATION - 0.01f);

            Assert.That(isComplete, Is.False);
        }

        [Test]
        public void IsComplete_AtDuration_ReturnsTrue()
        {
            bool isComplete = EnemyDeathCalculator.IsComplete(EnemyDeathCalculator.DURATION);

            Assert.That(isComplete, Is.True);
        }
    }
}
