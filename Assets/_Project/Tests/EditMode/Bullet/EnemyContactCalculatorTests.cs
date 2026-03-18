using NUnit.Framework;
using Unity.Mathematics;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class EnemyContactCalculatorTests
    {
        [Test]
        public void IsContact_WithinRadius_ReturnsTrue()
        {
            var playerPos = new float2(0f, 0f);
            var enemyPos = new float2(0.5f, 0f);
            Assert.That(EnemyContactCalculator.IsContact(playerPos, enemyPos, 1f), Is.True);
        }

        [Test]
        public void IsContact_ExactlyOnRadius_ReturnsTrue()
        {
            var playerPos = new float2(0f, 0f);
            var enemyPos = new float2(1f, 0f);
            Assert.That(EnemyContactCalculator.IsContact(playerPos, enemyPos, 1f), Is.True);
        }

        [Test]
        public void IsContact_OutsideRadius_ReturnsFalse()
        {
            var playerPos = new float2(0f, 0f);
            var enemyPos = new float2(2f, 0f);
            Assert.That(EnemyContactCalculator.IsContact(playerPos, enemyPos, 1f), Is.False);
        }
    }
}
