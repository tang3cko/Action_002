using NUnit.Framework;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class SpawnWaveCalculatorTests
    {
        [Test]
        public void Before30Seconds_AlwaysReturnsShooter()
        {
            Assert.That(SpawnWaveCalculator.SelectType(0f, 0f), Is.EqualTo(EnemyTypeId.Shooter));
            Assert.That(SpawnWaveCalculator.SelectType(0f, 0.99f), Is.EqualTo(EnemyTypeId.Shooter));
            Assert.That(SpawnWaveCalculator.SelectType(29.9f, 0.5f), Is.EqualTo(EnemyTypeId.Shooter));
        }

        [Test]
        public void Between30And60Seconds_CanReturnNWay()
        {
            // randomValue >= 0.7 → NWay
            Assert.That(SpawnWaveCalculator.SelectType(40f, 0.8f), Is.EqualTo(EnemyTypeId.NWay));
        }

        [Test]
        public void Between30And60Seconds_LowRandom_ReturnsShooter()
        {
            Assert.That(SpawnWaveCalculator.SelectType(40f, 0.3f), Is.EqualTo(EnemyTypeId.Shooter));
        }

        [Test]
        public void After60Seconds_LowRandom_ReturnsShooter()
        {
            // randomValue < 0.4 → Shooter
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.2f), Is.EqualTo(EnemyTypeId.Shooter));
        }

        [Test]
        public void After60Seconds_MidRandom_ReturnsNWay()
        {
            // 0.4 <= randomValue < 0.65 → NWay
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.5f), Is.EqualTo(EnemyTypeId.NWay));
        }

        [Test]
        public void After60Seconds_CanReturnRing()
        {
            // 0.65 <= randomValue < 0.85 → Ring
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.7f), Is.EqualTo(EnemyTypeId.Ring));
        }

        [Test]
        public void After60Seconds_HighRandom_ReturnsAnchor()
        {
            // randomValue >= 0.85 → Anchor
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.9f), Is.EqualTo(EnemyTypeId.Anchor));
        }
    }
}
