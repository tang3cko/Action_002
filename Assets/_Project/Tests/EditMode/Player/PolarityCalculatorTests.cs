using NUnit.Framework;
using Action002.Core;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class PolarityCalculatorTests
    {
        [Test]
        public void Toggle_White_ReturnsBlack()
        {
            var result = PolarityCalculator.Toggle(Polarity.White);
            Assert.That(result, Is.EqualTo(Polarity.Black));
        }

        [Test]
        public void Toggle_Black_ReturnsWhite()
        {
            var result = PolarityCalculator.Toggle(Polarity.Black);
            Assert.That(result, Is.EqualTo(Polarity.White));
        }

        [Test]
        public void IsSamePolarity_SameWhite_ReturnsTrue()
        {
            Assert.That(PolarityCalculator.IsSamePolarity(Polarity.White, 0), Is.True);
        }

        [Test]
        public void IsSamePolarity_DifferentPolarity_ReturnsFalse()
        {
            Assert.That(PolarityCalculator.IsSamePolarity(Polarity.White, 1), Is.False);
        }
    }
}
