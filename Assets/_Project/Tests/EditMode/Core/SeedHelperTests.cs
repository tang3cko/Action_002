using NUnit.Framework;
using Action002.Core;

namespace Action002.Tests.Core
{
    public class SeedHelperTests
    {
        // ── Normalize ──

        [Test]
        public void Normalize_Zero_ReturnsOne()
        {
            Assert.That(SeedHelper.Normalize(0u), Is.EqualTo(1u));
        }

        [Test]
        public void Normalize_NonZero_ReturnsSameValue()
        {
            Assert.That(SeedHelper.Normalize(42u), Is.EqualTo(42u));
        }

        [Test]
        public void Normalize_MaxValue_ReturnsSameValue()
        {
            Assert.That(SeedHelper.Normalize(uint.MaxValue), Is.EqualTo(uint.MaxValue));
        }

        // ── DeriveSpawnSeed / DerivePolaritySeed ──

        [Test]
        public void DeriveSpawnSeed_And_DerivePolaritySeed_ProduceDifferentSeeds()
        {
            uint runSeed = 12345u;
            uint spawnSeed = SeedHelper.DeriveSpawnSeed(runSeed);
            uint polaritySeed = SeedHelper.DerivePolaritySeed(runSeed);

            Assert.That(spawnSeed, Is.Not.EqualTo(polaritySeed));
        }

        [Test]
        public void DeriveSpawnSeed_NeverReturnsZero()
        {
            // 0xA5A5A5A5 ^ 0xA5A5A5A5 == 0 -> Normalize -> 1
            uint runSeed = 0xA5A5A5A5u;
            Assert.That(SeedHelper.DeriveSpawnSeed(runSeed), Is.EqualTo(1u));
        }

        [Test]
        public void DerivePolaritySeed_NeverReturnsZero()
        {
            // 0x5A5A5A5A ^ 0x5A5A5A5A == 0 -> Normalize -> 1
            uint runSeed = 0x5A5A5A5Au;
            Assert.That(SeedHelper.DerivePolaritySeed(runSeed), Is.EqualTo(1u));
        }

        [Test]
        public void DeriveSpawnSeed_Deterministic()
        {
            uint runSeed = 99999u;
            uint a = SeedHelper.DeriveSpawnSeed(runSeed);
            uint b = SeedHelper.DeriveSpawnSeed(runSeed);

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void DerivePolaritySeed_Deterministic()
        {
            uint runSeed = 99999u;
            uint a = SeedHelper.DerivePolaritySeed(runSeed);
            uint b = SeedHelper.DerivePolaritySeed(runSeed);

            Assert.That(a, Is.EqualTo(b));
        }

        // ── ResolveRunSeed ──

        [Test]
        public void ResolveRunSeed_FixedNonZero_ReturnsFixedSeed()
        {
            Assert.That(SeedHelper.ResolveRunSeed(12345u, 99999u), Is.EqualTo(12345u));
        }

        [Test]
        public void ResolveRunSeed_FixedZero_ReturnsFallbackTicks()
        {
            Assert.That(SeedHelper.ResolveRunSeed(0u, 67890u), Is.EqualTo(67890u));
        }

        [Test]
        public void ResolveRunSeed_FixedZero_FallbackZero_ReturnsOne()
        {
            Assert.That(SeedHelper.ResolveRunSeed(0u, 0u), Is.EqualTo(1u));
        }
    }
}
