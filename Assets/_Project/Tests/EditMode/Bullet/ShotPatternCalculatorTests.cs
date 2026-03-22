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

        // ── Spiral ──

        [Test]
        public void Spiral_Returns4Bullets_EquallySpacedFromBaseAngle()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.Spiral, 4, 15f, 2f);
            Span<BulletState> buf = stackalloc BulletState[4];
            float baseAngle = math.radians(45f);

            int written = ShotPatternCalculator.CalculateSpiral(buf, spec, float2.zero, baseAngle, 0, 10f);

            Assert.That(written, Is.EqualTo(4));

            for (int i = 0; i < written; i++)
            {
                float speed = math.length(buf[i].Velocity);
                Assert.That(speed, Is.EqualTo(2f).Within(0.01f));
                Assert.That(buf[i].Faction, Is.EqualTo(BulletFaction.Enemy));
            }

            // 最初の弾は baseAngle 方向
            float angle0 = math.atan2(buf[0].Velocity.y, buf[0].Velocity.x);
            Assert.That(angle0, Is.EqualTo(baseAngle).Within(0.01f));
        }

        [Test]
        public void Spiral_DifferentBaseAngle_ProducesDifferentDirections()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.Spiral, 4, 15f, 2f);
            Span<BulletState> bufA = stackalloc BulletState[4];
            Span<BulletState> bufB = stackalloc BulletState[4];

            ShotPatternCalculator.CalculateSpiral(bufA, spec, float2.zero, 0f, 0, 10f);
            ShotPatternCalculator.CalculateSpiral(bufB, spec, float2.zero, math.radians(30f), 0, 10f);

            // 異なる baseAngle なので方向が異なる
            float angleA = math.atan2(bufA[0].Velocity.y, bufA[0].Velocity.x);
            float angleB = math.atan2(bufB[0].Velocity.y, bufB[0].Velocity.x);
            Assert.That(math.abs(angleA - angleB), Is.GreaterThan(0.01f));
        }

        // ── RandomSpread ──

        [Test]
        public void RandomSpread_Returns8Bullets_WithJitter()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.RandomSpread, 8, 10f, 2f);
            Span<BulletState> buf = stackalloc BulletState[8];
            var rng = new Unity.Mathematics.Random(42);

            int written = ShotPatternCalculator.CalculateRandomSpread(buf, spec, float2.zero, ref rng, 0, 10f);

            Assert.That(written, Is.EqualTo(8));

            for (int i = 0; i < written; i++)
            {
                Assert.That(buf[i].Faction, Is.EqualTo(BulletFaction.Enemy));
            }
        }

        [Test]
        public void RandomSpread_SameSeed_ProducesReproducibleResults()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.RandomSpread, 4, 15f, 2f);
            Span<BulletState> bufA = stackalloc BulletState[4];
            Span<BulletState> bufB = stackalloc BulletState[4];

            var rngA = new Unity.Mathematics.Random(123);
            var rngB = new Unity.Mathematics.Random(123);

            ShotPatternCalculator.CalculateRandomSpread(bufA, spec, float2.zero, ref rngA, 0, 10f);
            ShotPatternCalculator.CalculateRandomSpread(bufB, spec, float2.zero, ref rngB, 0, 10f);

            for (int i = 0; i < 4; i++)
            {
                Assert.That(bufA[i].Velocity.x, Is.EqualTo(bufB[i].Velocity.x).Within(0.0001f));
                Assert.That(bufA[i].Velocity.y, Is.EqualTo(bufB[i].Velocity.y).Within(0.0001f));
            }
        }

        [Test]
        public void RandomSpread_DifferentSeeds_ProduceDifferentResults()
        {
            var spec = new ShotPatternSpec(ShotPatternKind.RandomSpread, 4, 15f, 2f);
            Span<BulletState> bufA = stackalloc BulletState[4];
            Span<BulletState> bufB = stackalloc BulletState[4];

            var rngA = new Unity.Mathematics.Random(42);
            var rngB = new Unity.Mathematics.Random(999);

            ShotPatternCalculator.CalculateRandomSpread(bufA, spec, float2.zero, ref rngA, 0, 10f);
            ShotPatternCalculator.CalculateRandomSpread(bufB, spec, float2.zero, ref rngB, 0, 10f);

            bool anyDifferent = false;
            for (int i = 0; i < 4; i++)
            {
                if (math.abs(bufA[i].Velocity.x - bufB[i].Velocity.x) > 0.001f ||
                    math.abs(bufA[i].Velocity.y - bufB[i].Velocity.y) > 0.001f)
                {
                    anyDifferent = true;
                    break;
                }
            }
            Assert.That(anyDifferent, Is.True);
        }
    }
}
