using System;
using NUnit.Framework;
using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class ShotPatternCalculatorTests
    {
        [Test]
        public void Aimed_Returns1Bullet_TowardPlayer()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.Aimed, 1, 0f, 3f);
            Span<BulletState> buf = stackalloc BulletState[1];

            int written = ShotPatternCalculator.Calculate(buf, spec, float2.zero, new float2(1f, 0f), 0, 10f);

            Assert.That(written, Is.EqualTo(1));
            Assert.That(buf[0].Velocity.x, Is.GreaterThan(0f));
            Assert.That(buf[0].Velocity.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(buf[0].Faction, Is.EqualTo(BulletFaction.Enemy));
            Assert.That(buf[0].Polarity, Is.EqualTo(0));
        }

        [Test]
        public void Aimed_TooCloseToPlayer_Returns0()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.Aimed, 1, 0f, 3f);
            Span<BulletState> buf = stackalloc BulletState[1];

            int written = ShotPatternCalculator.Calculate(buf, spec, float2.zero, new float2(0.001f, 0f), 0, 10f);

            Assert.That(written, Is.EqualTo(0));
        }

        [Test]
        public void NWay_Returns3Bullets_CenterAimedAtPlayer()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.NWay, 3, 30f, 2.5f);
            Span<BulletState> buf = stackalloc BulletState[3];

            int written = ShotPatternCalculator.Calculate(buf, spec, float2.zero, new float2(1f, 0f), 1, 10f);

            Assert.That(written, Is.EqualTo(3));

            // Center bullet (index 1) should aim toward player (positive x)
            Assert.That(buf[1].Velocity.x, Is.GreaterThan(0f));
            Assert.That(buf[1].Velocity.y, Is.EqualTo(0f).Within(0.1f));

            // All bullets should have enemy faction and black polarity
            for (int i = 0; i < written; i++)
            {
                Assert.That(buf[i].Faction, Is.EqualTo(BulletFaction.Enemy));
                Assert.That(buf[i].Polarity, Is.EqualTo(1));
            }
        }

        [Test]
        public void NWay_BulletsSpreadSymmetrically()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.NWay, 3, 60f, 2f);
            Span<BulletState> buf = stackalloc BulletState[3];

            ShotPatternCalculator.Calculate(buf, spec, float2.zero, new float2(1f, 0f), 0, 10f);

            // First and last bullets should have equal but opposite y components
            Assert.That(buf[0].Velocity.y, Is.EqualTo(-buf[2].Velocity.y).Within(0.001f));
        }

        [Test]
        public void Ring_Returns8Bullets_EquallySpaced()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.Ring, 8, 0f, 2f);
            Span<BulletState> buf = stackalloc BulletState[8];

            int written = ShotPatternCalculator.Calculate(buf, spec, float2.zero, float2.zero, 0, 10f);

            Assert.That(written, Is.EqualTo(8));

            // All bullets should have the same speed
            for (int i = 0; i < written; i++)
            {
                float speed = math.length(buf[i].Velocity);
                Assert.That(speed, Is.EqualTo(2f).Within(0.01f));
            }

            // Adjacent bullets should have ~45 degree angle difference (360/8)
            float angle0 = math.atan2(buf[0].Velocity.y, buf[0].Velocity.x);
            float angle1 = math.atan2(buf[1].Velocity.y, buf[1].Velocity.x);
            float diff = math.abs(angle1 - angle0);
            Assert.That(diff, Is.EqualTo(math.PI / 4f).Within(0.01f));
        }

        [Test]
        public void BufferTooSmall_ReturnsClampedCount()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.Ring, 8, 0f, 2f);
            Span<BulletState> buf = stackalloc BulletState[3];

            int written = ShotPatternCalculator.Calculate(buf, spec, float2.zero, float2.zero, 0, 10f);

            Assert.That(written, Is.EqualTo(3));
        }
    }
}
