using NUnit.Framework;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class EnemySpawnCalculatorTests
    {
        [Test]
        public void CalculateScale_BeforeSpawnStart_ClampsToZeroProgress()
        {
            float scale = EnemySpawnCalculator.CalculateScale(-0.1f, 2f);

            Assert.That(scale, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateScale_AfterDuration_ReturnsBaseScale()
        {
            float scale = EnemySpawnCalculator.CalculateScale(1f, 2f);

            Assert.That(scale, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void IsComplete_BeforeDuration_ReturnsFalse()
        {
            bool isComplete = EnemySpawnCalculator.IsComplete(0.19f);

            Assert.That(isComplete, Is.False);
        }

        [Test]
        public void IsComplete_AtDuration_ReturnsTrue()
        {
            bool isComplete = EnemySpawnCalculator.IsComplete(0.2f);

            Assert.That(isComplete, Is.True);
        }
    }
}
