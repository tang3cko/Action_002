using System;
using NUnit.Framework;
using Unity.Mathematics;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class EnemyRotationCalculatorTests
    {
        [Test]
        public void CalculateAngle_ShooterFacesPlayer()
        {
            float angle = EnemyRotationCalculator.CalculateAngle(
                EnemyTypeId.Shooter,
                float2.zero,
                new float2(1f, 0f),
                0f);

            Assert.That(angle, Is.EqualTo(-90f));
        }

        [Test]
        public void CalculateShooterAngle_WhenPlayerOverlapsEnemy_ReturnsZero()
        {
            float angle = EnemyRotationCalculator.CalculateShooterAngle(float2.zero, float2.zero);

            Assert.That(angle, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateNWayAngle_ScalesWithTime()
        {
            float angle = EnemyRotationCalculator.CalculateNWayAngle(2f);

            Assert.That(angle, Is.EqualTo(90f));
        }

        [Test]
        public void CalculateRingAngle_ScalesWithTime()
        {
            float angle = EnemyRotationCalculator.CalculateRingAngle(1.5f);

            Assert.That(angle, Is.EqualTo(135f));
        }

        [Test]
        public void CalculateAnchorAngle_UsesSineWobble()
        {
            float angle = EnemyRotationCalculator.CalculateAnchorAngle(math.PI * 0.25f);

            Assert.That(angle, Is.EqualTo(15f).Within(0.0001f));
        }

        [Test]
        public void CalculateAngle_WithUnknownType_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                EnemyRotationCalculator.CalculateAngle((EnemyTypeId)999, float2.zero, float2.zero, 0f));
        }
    }
}
