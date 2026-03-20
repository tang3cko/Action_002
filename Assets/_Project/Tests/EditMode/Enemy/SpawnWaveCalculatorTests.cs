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
        public void After60Seconds_CanReturnRing()
        {
            // randomValue >= 0.8 → Ring
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.9f), Is.EqualTo(EnemyTypeId.Ring));
        }

        [Test]
        public void After60Seconds_MidRandom_ReturnsNWay()
        {
            // 0.5 <= randomValue < 0.8 → NWay
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.6f), Is.EqualTo(EnemyTypeId.NWay));
        }

        [Test]
        public void After60Seconds_LowRandom_ReturnsShooter()
        {
            // randomValue < 0.5 → Shooter
            Assert.That(SpawnWaveCalculator.SelectType(90f, 0.2f), Is.EqualTo(EnemyTypeId.Shooter));
        }
    }
}
