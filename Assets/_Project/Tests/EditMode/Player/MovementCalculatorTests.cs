using NUnit.Framework;
using Unity.Mathematics;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class MovementCalculatorTests
    {
        [Test]
        public void CalculateVelocity_UnitInput_ReturnsNormalizedSpeed()
        {
            var result = MovementCalculator.CalculateVelocity(new float2(1f, 0f), 5f);
            Assert.That(result.x, Is.EqualTo(5f).Within(0.01f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void CalculateVelocity_DiagonalInput_NormalizesToSpeed()
        {
            var result = MovementCalculator.CalculateVelocity(new float2(1f, 1f), 5f);
            float magnitude = math.length(result);
            Assert.That(magnitude, Is.EqualTo(5f).Within(0.01f));
        }

        [Test]
        public void CalculateVelocity_ZeroInput_ReturnsZero()
        {
            var result = MovementCalculator.CalculateVelocity(float2.zero, 5f);
            Assert.That(result.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ClampPosition_InsideBounds_Unchanged()
        {
            var pos = new float2(5f, 3f);
            var result = MovementCalculator.ClampPosition(pos, new float2(-10f, -10f), new float2(10f, 10f));
            Assert.That(result.x, Is.EqualTo(5f).Within(0.01f));
            Assert.That(result.y, Is.EqualTo(3f).Within(0.01f));
        }

        [Test]
        public void ClampPosition_OutsideBounds_ClampedToEdge()
        {
            var pos = new float2(20f, -15f);
            var result = MovementCalculator.ClampPosition(pos, new float2(-10f, -10f), new float2(10f, 10f));
            Assert.That(result.x, Is.EqualTo(10f).Within(0.01f));
            Assert.That(result.y, Is.EqualTo(-10f).Within(0.01f));
        }
    }
}
