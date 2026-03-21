using NUnit.Framework;
using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class BulletCollisionCalculatorTests
    {
        [Test]
        public void IsPlayerBullet_FactionZero_ReturnsTrue()
        {
            Assert.That(BulletCollisionCalculator.IsPlayerBullet(BulletFaction.Player), Is.True);
        }

        [Test]
        public void IsPlayerBullet_FactionOne_ReturnsFalse()
        {
            Assert.That(BulletCollisionCalculator.IsPlayerBullet(BulletFaction.Enemy), Is.False);
        }

        [Test]
        public void IsWithinRadius_Inside_ReturnsTrue()
        {
            var a = new float2(0f, 0f);
            var b = new float2(0.5f, 0f);
            Assert.That(BulletCollisionCalculator.IsWithinRadius(a, b, 1f), Is.True);
        }

        [Test]
        public void IsWithinRadius_ExactlyOnBoundary_ReturnsTrue()
        {
            var a = new float2(0f, 0f);
            var b = new float2(1f, 0f);
            Assert.That(BulletCollisionCalculator.IsWithinRadius(a, b, 1f), Is.True);
        }

        [Test]
        public void IsWithinRadius_Outside_ReturnsFalse()
        {
            var a = new float2(0f, 0f);
            var b = new float2(2f, 0f);
            Assert.That(BulletCollisionCalculator.IsWithinRadius(a, b, 1f), Is.False);
        }

        [Test]
        public void ShouldAbsorb_SamePolarityWithinRadius_ReturnsTrue()
        {
            var bulletPos = new float2(0.3f, 0f);
            var playerPos = new float2(0f, 0f);
            Assert.That(BulletCollisionCalculator.ShouldAbsorb(true, bulletPos, playerPos, 1f), Is.True);
        }

        [Test]
        public void ShouldAbsorb_SamePolarityOutsideRadius_ReturnsFalse()
        {
            var bulletPos = new float2(5f, 0f);
            var playerPos = new float2(0f, 0f);
            Assert.That(BulletCollisionCalculator.ShouldAbsorb(true, bulletPos, playerPos, 1f), Is.False);
        }

        [Test]
        public void ShouldAbsorb_DifferentPolarityWithinRadius_ReturnsFalse()
        {
            var bulletPos = new float2(0.3f, 0f);
            var playerPos = new float2(0f, 0f);
            Assert.That(BulletCollisionCalculator.ShouldAbsorb(false, bulletPos, playerPos, 1f), Is.False);
        }

        [Test]
        public void ShouldDamagePlayer_DifferentPolarityWithinRadius_ReturnsTrue()
        {
            var bulletPos = new float2(0.3f, 0f);
            var playerPos = new float2(0f, 0f);
            Assert.That(BulletCollisionCalculator.ShouldDamagePlayer(false, bulletPos, playerPos, 0.5f), Is.True);
        }

        [Test]
        public void ShouldDamagePlayer_SamePolarityWithinRadius_ReturnsFalse()
        {
            var bulletPos = new float2(0.3f, 0f);
            var playerPos = new float2(0f, 0f);
            Assert.That(BulletCollisionCalculator.ShouldDamagePlayer(true, bulletPos, playerPos, 0.5f), Is.False);
        }

        [Test]
        public void ShouldDamagePlayer_DifferentPolarityOutsideRadius_ReturnsFalse()
        {
            var bulletPos = new float2(5f, 0f);
            var playerPos = new float2(0f, 0f);
            Assert.That(BulletCollisionCalculator.ShouldDamagePlayer(false, bulletPos, playerPos, 0.5f), Is.False);
        }

        [Test]
        public void CalculateRemainingHp_SubtractsDamage()
        {
            Assert.That(BulletCollisionCalculator.CalculateRemainingHp(10, 3), Is.EqualTo(7));
        }

        [Test]
        public void CalculateRemainingHp_DamageExceedsHp_ReturnsNegative()
        {
            Assert.That(BulletCollisionCalculator.CalculateRemainingHp(2, 5), Is.EqualTo(-3));
        }

        [Test]
        public void IsEnemyKilled_ZeroHp_ReturnsTrue()
        {
            Assert.That(BulletCollisionCalculator.IsEnemyKilled(0), Is.True);
        }

        [Test]
        public void IsEnemyKilled_NegativeHp_ReturnsTrue()
        {
            Assert.That(BulletCollisionCalculator.IsEnemyKilled(-1), Is.True);
        }

        [Test]
        public void IsEnemyKilled_PositiveHp_ReturnsFalse()
        {
            Assert.That(BulletCollisionCalculator.IsEnemyKilled(5), Is.False);
        }
    }
}
