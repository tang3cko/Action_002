using NUnit.Framework;
using Unity.Mathematics;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class BulletBoundsCalculatorTests
    {
        private static readonly float2 Min = new float2(-5f, -5f);
        private static readonly float2 Max = new float2(5f, 5f);
        private const float Margin = 1f;

        [Test]
        public void IsOutsideBounds_InsideCenter_ReturnsFalse()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(0f, 0f), Min, Max, Margin);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsOutsideBounds_InsideEdge_ReturnsFalse()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(5f, 5f), Min, Max, Margin);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsOutsideBounds_InsideMarginZone_ReturnsFalse()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(5.5f, 0f), Min, Max, Margin);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsOutsideBounds_OnBoundary_ReturnsFalse()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(6f, 0f), Min, Max, Margin);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsOutsideBounds_BeyondRight_ReturnsTrue()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(6.1f, 0f), Min, Max, Margin);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsOutsideBounds_BeyondLeft_ReturnsTrue()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(-6.1f, 0f), Min, Max, Margin);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsOutsideBounds_BeyondTop_ReturnsTrue()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(0f, 6.1f), Min, Max, Margin);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsOutsideBounds_BeyondBottom_ReturnsTrue()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(0f, -6.1f), Min, Max, Margin);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsOutsideBounds_ZeroMargin_ExactBoundary_ReturnsFalse()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(5f, 5f), Min, Max, 0f);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsOutsideBounds_ZeroMargin_SlightlyOutside_ReturnsTrue()
        {
            bool result = BulletBoundsCalculator.IsOutsideBounds(new float2(5.01f, 0f), Min, Max, 0f);
            Assert.That(result, Is.True);
        }
    }
}
