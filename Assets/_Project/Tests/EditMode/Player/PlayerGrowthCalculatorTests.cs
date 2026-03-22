using NUnit.Framework;
using Action002.Player.Data;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class PlayerGrowthCalculatorTests
    {
        // ── CreateDefault ──

        [Test]
        public void CreateDefault_ReturnsLevel0()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            Assert.That(state.Level, Is.EqualTo(0));
        }

        [Test]
        public void CreateDefault_ReturnsBulletCount1()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            Assert.That(state.BulletCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateDefault_ReturnsMoveSpeedMultiplier1()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void CreateDefault_ReturnsBulletSpeedMultiplier1()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            Assert.That(state.BulletSpeedMultiplier, Is.EqualTo(1f));
        }

        // ── ShouldLevelUp ──

        [Test]
        public void ShouldLevelUp_GaugeAt1_ReturnsTrue()
        {
            Assert.That(PlayerGrowthCalculator.ShouldLevelUp(1f), Is.True);
        }

        [Test]
        public void ShouldLevelUp_GaugeAbove1_ReturnsTrue()
        {
            Assert.That(PlayerGrowthCalculator.ShouldLevelUp(1.5f), Is.True);
        }

        [Test]
        public void ShouldLevelUp_GaugeBelow1_ReturnsFalse()
        {
            Assert.That(PlayerGrowthCalculator.ShouldLevelUp(0.99f), Is.False);
        }

        [Test]
        public void ShouldLevelUp_GaugeAt0_ReturnsFalse()
        {
            Assert.That(PlayerGrowthCalculator.ShouldLevelUp(0f), Is.False);
        }

        // ── ApplyLevelUp ──

        [Test]
        public void ApplyLevelUp_Level0To1_BulletCountIs2()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            state = PlayerGrowthCalculator.ApplyLevelUp(state);
            Assert.That(state.Level, Is.EqualTo(1));
            Assert.That(state.BulletCount, Is.EqualTo(2));
        }

        [Test]
        public void ApplyLevelUp_Level1To2_MoveSpeedIncreasedBy10Percent()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            state = PlayerGrowthCalculator.ApplyLevelUp(state); // -> Level 1
            state = PlayerGrowthCalculator.ApplyLevelUp(state); // -> Level 2
            Assert.That(state.Level, Is.EqualTo(2));
            Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1.10f).Within(0.001f));
        }

        [Test]
        public void ApplyLevelUp_Level2To3_BulletCountIs3()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            for (int i = 0; i < 3; i++)
                state = PlayerGrowthCalculator.ApplyLevelUp(state);
            Assert.That(state.Level, Is.EqualTo(3));
            Assert.That(state.BulletCount, Is.EqualTo(3));
        }

        [Test]
        public void ApplyLevelUp_Level3To4_BulletSpeedIncreasedBy15Percent()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            for (int i = 0; i < 4; i++)
                state = PlayerGrowthCalculator.ApplyLevelUp(state);
            Assert.That(state.Level, Is.EqualTo(4));
            Assert.That(state.BulletSpeedMultiplier, Is.EqualTo(1.15f).Within(0.001f));
        }

        [Test]
        public void ApplyLevelUp_Level4To5_BulletCountIs4()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            for (int i = 0; i < 5; i++)
                state = PlayerGrowthCalculator.ApplyLevelUp(state);
            Assert.That(state.Level, Is.EqualTo(5));
            Assert.That(state.BulletCount, Is.EqualTo(4));
        }

        [Test]
        public void ApplyLevelUp_Level6Plus_BulletCountCappedAt4_MoveSpeedIncrements()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            for (int i = 0; i < 6; i++)
                state = PlayerGrowthCalculator.ApplyLevelUp(state);

            Assert.That(state.Level, Is.EqualTo(6));
            Assert.That(state.BulletCount, Is.EqualTo(4));
            // MoveSpeed: 1.0 + 0.10 (lv2) + 0.05 (lv6) = 1.15
            Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1.15f).Within(0.001f));
        }

        [Test]
        public void ApplyLevelUp_Level7_MoveSpeedIncrementsFurther()
        {
            var state = PlayerGrowthCalculator.CreateDefault();
            for (int i = 0; i < 7; i++)
                state = PlayerGrowthCalculator.ApplyLevelUp(state);

            Assert.That(state.Level, Is.EqualTo(7));
            Assert.That(state.BulletCount, Is.EqualTo(4));
            // MoveSpeed: 1.0 + 0.10 (lv2) + 0.05 (lv6) + 0.05 (lv7) = 1.20
            Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1.20f).Within(0.001f));
        }
    }
}
