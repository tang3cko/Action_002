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
        public void GetSpawnInterval_BeforeOvertime_RespectsMinimum()
        {
            // elapsed=100 is before 120s overtime threshold, so multiplier=1
            float result = SpawnCalculator.GetSpawnInterval(0.75f, 100f, 0.2f);
            Assert.That(result, Is.EqualTo(0.2f));
        }

        [Test]
        public void GetSpawnInterval_DuringOvertime_ShrinksBelowMinimum()
        {
            // elapsed=1000: overtime multiplier > 1, so result < minInterval
            float result = SpawnCalculator.GetSpawnInterval(0.75f, 1000f, 0.2f);
            Assert.That(result, Is.LessThan(0.2f));
        }

        // ── GetOvertimeMultiplier ──

        [Test]
        public void GetOvertimeMultiplier_Before120s_Returns1()
        {
            Assert.That(SpawnCalculator.GetOvertimeMultiplier(0f), Is.EqualTo(1f));
            Assert.That(SpawnCalculator.GetOvertimeMultiplier(60f), Is.EqualTo(1f));
            Assert.That(SpawnCalculator.GetOvertimeMultiplier(120f), Is.EqualTo(1f));
        }

        [Test]
        public void GetOvertimeMultiplier_At180s_ReturnsExpected()
        {
            // overtime=60, 1 + log2(1 + 60/60) = 1 + log2(2) = 1 + 1 = 2
            float result = SpawnCalculator.GetOvertimeMultiplier(180f);
            Assert.That(result, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void OvertimeSpawnAndSpeedMultipliers_At180s_ResultInExpectedSpeedMultiplier()
        {
            // 120s is the overtime baseline: spawn interval is already clamped to min and overtime multiplier is 1.
            // intervalAt120 = max(0.75 - 120*0.01, 0.2) / 1.0 = 0.2
            // intervalAt180 = max(0.75 - 180*0.01, 0.2) / 2.0 = 0.1
            // spawnRateMultiplier = 0.2 / 0.1 = 2.0
            // overtimeMultiplier  = 2.0
            // effective (spawn rate x enemy speed) = 2.0 * 2.0 = 4.0
            float intervalAt120 = SpawnCalculator.GetSpawnInterval(0.75f, 120f, 0.2f);
            float intervalAt180 = SpawnCalculator.GetSpawnInterval(0.75f, 180f, 0.2f);
            float overtimeMultiplierAt180 = SpawnCalculator.GetOvertimeMultiplier(180f);

            float spawnRateMultiplierAt180 = intervalAt120 / intervalAt180;
            float effectiveSpeedMultiplierAt180 = spawnRateMultiplierAt180 * overtimeMultiplierAt180;

            Assert.That(effectiveSpeedMultiplierAt180, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void GetOvertimeMultiplier_IncreasesOverTime()
        {
            float at180 = SpawnCalculator.GetOvertimeMultiplier(180f);
            float at300 = SpawnCalculator.GetOvertimeMultiplier(300f);
            Assert.That(at300, Is.GreaterThan(at180));
        }
    }
}
