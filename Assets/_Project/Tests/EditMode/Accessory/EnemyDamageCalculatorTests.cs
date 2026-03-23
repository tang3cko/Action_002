using NUnit.Framework;
using Action002.Enemy.Logic;

namespace Action002.Tests.Accessory
{
    public class EnemyDamageCalculatorTests
    {
        [Test]
        public void ApplyDamage_DamageReducesHp()
        {
            var result = EnemyDamageCalculator.ApplyDamage(10, 3);

            Assert.That(result.RemainingHp, Is.EqualTo(7));
            Assert.That(result.IsKilled, Is.False);
        }

        [Test]
        public void ApplyDamage_DamageEqualsHp_IsKilled()
        {
            var result = EnemyDamageCalculator.ApplyDamage(5, 5);

            Assert.That(result.RemainingHp, Is.EqualTo(0));
            Assert.That(result.IsKilled, Is.True);
        }

        [Test]
        public void ApplyDamage_DamageExceedsHp_IsKilled()
        {
            var result = EnemyDamageCalculator.ApplyDamage(3, 10);

            Assert.That(result.RemainingHp, Is.EqualTo(-7));
            Assert.That(result.IsKilled, Is.True);
        }

        [Test]
        public void ApplyDamage_ZeroDamage_NotKilled()
        {
            var result = EnemyDamageCalculator.ApplyDamage(5, 0);

            Assert.That(result.RemainingHp, Is.EqualTo(5));
            Assert.That(result.IsKilled, Is.False);
        }

        [Test]
        public void ApplyDamage_OneDamage_OneHp_IsKilled()
        {
            var result = EnemyDamageCalculator.ApplyDamage(1, 1);

            Assert.That(result.RemainingHp, Is.EqualTo(0));
            Assert.That(result.IsKilled, Is.True);
        }
    }
}
