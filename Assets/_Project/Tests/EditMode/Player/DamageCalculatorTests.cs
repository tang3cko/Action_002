using NUnit.Framework;
using Action002.Player.Data;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class DamageCalculatorTests
    {
        [Test]
        public void ApplyDamage_ReducesHp()
        {
            var state = new PlayerState { Hp = 5, InvincibleTimer = 0f, ComboCount = 3, ComboMultiplier = 1.3f };
            state = DamageCalculator.ApplyDamage(state, 1f);
            Assert.That(state.Hp, Is.EqualTo(4));
        }

        [Test]
        public void ApplyDamage_SetsInvincibleTimer()
        {
            var state = new PlayerState { Hp = 5, InvincibleTimer = 0f };
            state = DamageCalculator.ApplyDamage(state, 1.5f);
            Assert.That(state.InvincibleTimer, Is.EqualTo(1.5f));
        }

        [Test]
        public void ApplyDamage_ResetsCombo()
        {
            var state = new PlayerState { Hp = 5, InvincibleTimer = 0f, ComboCount = 10, ComboMultiplier = 2f };
            state = DamageCalculator.ApplyDamage(state, 1f);
            Assert.That(state.ComboCount, Is.EqualTo(0));
            Assert.That(state.ComboMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void ApplyDamage_WhileInvincible_DoesNothing()
        {
            var state = new PlayerState { Hp = 5, InvincibleTimer = 0.5f };
            state = DamageCalculator.ApplyDamage(state, 1f);
            Assert.That(state.Hp, Is.EqualTo(5));
        }

        [Test]
        public void TickInvincibility_DecreasesTimer()
        {
            var state = new PlayerState { InvincibleTimer = 1f };
            state = DamageCalculator.TickInvincibility(state, 0.3f);
            Assert.That(state.InvincibleTimer, Is.EqualTo(0.7f).Within(0.001f));
        }

        [Test]
        public void IsDead_HpZero_ReturnsTrue()
        {
            var state = new PlayerState { Hp = 0 };
            Assert.That(DamageCalculator.IsDead(state), Is.True);
        }

        [Test]
        public void IsDead_HpPositive_ReturnsFalse()
        {
            var state = new PlayerState { Hp = 1 };
            Assert.That(DamageCalculator.IsDead(state), Is.False);
        }
    }
}
