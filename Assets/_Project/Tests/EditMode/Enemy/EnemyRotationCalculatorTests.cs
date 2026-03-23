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
                new float2(0f, 1f),
                0f);

            Assert.That(angle, Is.EqualTo(180f));
        }

        [Test]
        public void CalculateShooterAngle_PlayerToRight_Returns90()
        {
            float angle = EnemyRotationCalculator.CalculateShooterAngle(float2.zero, new float2(1f, 0f));

            Assert.That(angle, Is.EqualTo(90f));
        }

        [Test]
        public void CalculateShooterAngle_WhenPlayerOverlapsEnemy_ReturnsZero()
        {
            float angle = EnemyRotationCalculator.CalculateShooterAngle(float2.zero, float2.zero);

            Assert.That(angle, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateAngle_NWay_ReturnsAccumulatedRotation()
        {
            float rotationAngle = 123.4f;
            float angle = EnemyRotationCalculator.CalculateAngle(
                EnemyTypeId.NWay, float2.zero, float2.zero, rotationAngle);

            Assert.That(angle, Is.EqualTo(rotationAngle));
        }

        [Test]
        public void CalculateAngle_Ring_ReturnsAccumulatedRotation()
        {
            float rotationAngle = -45.6f;
            float angle = EnemyRotationCalculator.CalculateAngle(
                EnemyTypeId.Ring, float2.zero, float2.zero, rotationAngle);

            Assert.That(angle, Is.EqualTo(rotationAngle));
        }

        [Test]
        public void CalculateAngle_Anchor_ReturnsAccumulatedRotation()
        {
            float rotationAngle = 78.9f;
            float angle = EnemyRotationCalculator.CalculateAngle(
                EnemyTypeId.Anchor, float2.zero, float2.zero, rotationAngle);

            Assert.That(angle, Is.EqualTo(rotationAngle));
        }

        [Test]
        public void GetRotationSpeed_Shooter_ReturnsZero()
        {
            Assert.That(EnemyRotationCalculator.GetRotationSpeed(EnemyTypeId.Shooter), Is.EqualTo(0f));
        }

        [Test]
        public void GetRotationSpeed_NWay_ReturnsExpectedSpeed()
        {
            Assert.That(EnemyRotationCalculator.GetRotationSpeed(EnemyTypeId.NWay),
                Is.EqualTo(EnemyRotationCalculator.N_WAY_DEGREES_PER_SECOND));
        }

        [Test]
        public void GetRotationSpeed_Ring_ReturnsExpectedSpeed()
        {
            Assert.That(EnemyRotationCalculator.GetRotationSpeed(EnemyTypeId.Ring),
                Is.EqualTo(80f));
        }

        [Test]
        public void GetRotationSpeed_Anchor_ReturnsExpectedSpeed()
        {
            Assert.That(EnemyRotationCalculator.GetRotationSpeed(EnemyTypeId.Anchor),
                Is.EqualTo(90f));
        }

        [Test]
        public void CalculateAngle_WithUnknownType_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                EnemyRotationCalculator.CalculateAngle((EnemyTypeId)999, float2.zero, float2.zero, 0f));
        }

        // --- GetStepAngle ---

        [Test]
        public void GetStepAngle_Ring_Returns60()
        {
            Assert.That(EnemyRotationCalculator.GetStepAngle(EnemyTypeId.Ring),
                Is.EqualTo(EnemyRotationCalculator.RING_STEP_ANGLE));
        }

        [Test]
        public void GetStepAngle_Anchor_Returns90()
        {
            Assert.That(EnemyRotationCalculator.GetStepAngle(EnemyTypeId.Anchor),
                Is.EqualTo(EnemyRotationCalculator.ANCHOR_STEP_ANGLE));
        }

        [Test]
        public void GetStepAngle_Shooter_ReturnsZero()
        {
            Assert.That(EnemyRotationCalculator.GetStepAngle(EnemyTypeId.Shooter), Is.EqualTo(0f));
        }

        [Test]
        public void GetStepAngle_NWay_ReturnsZero()
        {
            Assert.That(EnemyRotationCalculator.GetStepAngle(EnemyTypeId.NWay), Is.EqualTo(0f));
        }

        // --- GetHoldRatio ---

        [Test]
        public void GetHoldRatio_Ring_IsPositiveAndLessThanOne()
        {
            float ratio = EnemyRotationCalculator.GetHoldRatio(EnemyTypeId.Ring);
            Assert.That(ratio, Is.GreaterThan(0f));
            Assert.That(ratio, Is.LessThan(1f));
        }

        [Test]
        public void GetHoldRatio_Anchor_IsPositiveAndLessThanOne()
        {
            float ratio = EnemyRotationCalculator.GetHoldRatio(EnemyTypeId.Anchor);
            Assert.That(ratio, Is.GreaterThan(0f));
            Assert.That(ratio, Is.LessThan(1f));
        }

        [Test]
        public void GetHoldRatio_Shooter_ReturnsZero()
        {
            Assert.That(EnemyRotationCalculator.GetHoldRatio(EnemyTypeId.Shooter), Is.EqualTo(0f));
        }
    }
}
