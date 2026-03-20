using NUnit.Framework;
using Unity.Mathematics;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class EnemyContactCalculatorTests
    {
        [Test]
        public void IsContact_WithinCombinedRadius_ReturnsTrue()
        {
            var playerPos = new float2(0f, 0f);
            var enemyPos = new float2(0.5f, 0f);
            Assert.That(EnemyContactCalculator.IsContact(playerPos, enemyPos, 0.3f, 0.5f), Is.True);
        }

        [Test]
        public void IsContact_ExactlyOnCombinedRadius_ReturnsTrue()
        {
            var playerPos = new float2(0f, 0f);
            var enemyPos = new float2(0.79f, 0f);
            Assert.That(EnemyContactCalculator.IsContact(playerPos, enemyPos, 0.3f, 0.5f), Is.True);
        }

        [Test]
        public void IsContact_OutsideCombinedRadius_ReturnsFalse()
        {
            var playerPos = new float2(0f, 0f);
            var enemyPos = new float2(2f, 0f);
            Assert.That(EnemyContactCalculator.IsContact(playerPos, enemyPos, 0.3f, 0.5f), Is.False);
        }
    }
}
