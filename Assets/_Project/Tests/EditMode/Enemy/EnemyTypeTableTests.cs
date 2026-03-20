using System;
using NUnit.Framework;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;

namespace Action002.Tests.Enemy
{
    public class EnemyTypeTableTests
    {
        [Test]
        public void Get_Shooter_ReturnsValidSpec()
        {
            var spec = EnemyTypeTable.Get(EnemyTypeId.Shooter);

            Assert.That(spec.Hp, Is.EqualTo(1));
            Assert.That(spec.SpeedMultiplier, Is.EqualTo(1.0f));
            Assert.That(spec.ShotPattern.Count, Is.EqualTo(1));
        }

        [Test]
        public void Get_NWay_ReturnsValidSpec()
        {
            var spec = EnemyTypeTable.Get(EnemyTypeId.NWay);

            Assert.That(spec.Hp, Is.EqualTo(1));
            Assert.That(spec.ShotPattern.Count, Is.EqualTo(3));
            Assert.That(spec.ShotPattern.ArcDegrees, Is.GreaterThan(0f));
        }

        [Test]
        public void Get_Ring_ReturnsHigherHp()
        {
            var spec = EnemyTypeTable.Get(EnemyTypeId.Ring);

            Assert.That(spec.Hp, Is.EqualTo(3));
            Assert.That(spec.ShotPattern.Count, Is.EqualTo(8));
            Assert.That(spec.VisualScale, Is.GreaterThan(1f));
            Assert.That(spec.CollisionRadius, Is.GreaterThan(0.5f));
        }

        [Test]
        public void Get_UndefinedId_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EnemyTypeTable.Get((EnemyTypeId)999));
        }

        [TestCase(EnemyTypeId.Shooter)]
        [TestCase(EnemyTypeId.NWay)]
        [TestCase(EnemyTypeId.Ring)]
        public void Get_AllTypes_HavePositiveCollisionRadius(EnemyTypeId id)
        {
            var spec = EnemyTypeTable.Get(id);
            Assert.That(spec.CollisionRadius, Is.GreaterThan(0f));
        }

        [TestCase(EnemyTypeId.Shooter)]
        [TestCase(EnemyTypeId.NWay)]
        [TestCase(EnemyTypeId.Ring)]
        public void Get_AllTypes_HavePositiveVisualScale(EnemyTypeId id)
        {
            var spec = EnemyTypeTable.Get(id);
            Assert.That(spec.VisualScale, Is.GreaterThan(0f));
        }
    }
}
