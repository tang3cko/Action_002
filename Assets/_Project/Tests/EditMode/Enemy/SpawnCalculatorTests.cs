using NUnit.Framework;
using Unity.Mathematics;
using Action002.Core;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class SpawnCalculatorTests
    {
        [Test]
        public void GetSpawnPosition_IsAtCorrectRadius()
        {
            float2 center = new float2(0, 0);
            float radius = 10f;
            float2 pos = SpawnCalculator.GetSpawnPosition(center, radius, 0f);
            Assert.That(math.distance(center, pos), Is.EqualTo(radius).Within(0.01f));
        }

        [Test]
        public void GetRandomPolarity_LowValue_ReturnsWhite()
        {
            Assert.That(SpawnCalculator.GetRandomPolarity(0.2f), Is.EqualTo(Polarity.White));
        }

        [Test]
        public void GetRandomPolarity_HighValue_ReturnsBlack()
        {
            Assert.That(SpawnCalculator.GetRandomPolarity(0.7f), Is.EqualTo(Polarity.Black));
        }

        [Test]
        public void GetSpawnInterval_DecreasesOverTime()
        {
            float early = SpawnCalculator.GetSpawnInterval(0.75f, 10f, 0.2f);
            float late = SpawnCalculator.GetSpawnInterval(0.75f, 50f, 0.2f);
            Assert.That(late, Is.LessThan(early));
        }

        [Test]
        public void GetSpawnInterval_RespectsMinimum()
        {
            float result = SpawnCalculator.GetSpawnInterval(0.75f, 1000f, 0.2f);
            Assert.That(result, Is.EqualTo(0.2f));
        }
    }
}
