using NUnit.Framework;
using Action002.Player.Data;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class ComboCalculatorTests
    {
        [Test]
        public void TickComboTimer_DecrementsTimer()
        {
            var state = new PlayerState { ComboCount = 3, ComboTimer = 2f };
            state = ComboCalculator.TickComboTimer(state, 0.5f);
            Assert.That(state.ComboTimer, Is.EqualTo(1.5f).Within(0.01f));
            Assert.That(state.ComboCount, Is.EqualTo(3));
        }

        [Test]
        public void TickComboTimer_ResetsComboWhenTimerExpires()
        {
            var state = new PlayerState { ComboCount = 5, ComboMultiplier = 2.5f, ComboTimer = 0.3f };
            state = ComboCalculator.TickComboTimer(state, 0.5f);
            Assert.That(state.ComboCount, Is.EqualTo(0));
            Assert.That(state.ComboMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void TickComboTimer_DoesNothingWhenComboCountIsZero()
        {
            var state = new PlayerState { ComboCount = 0, ComboTimer = 0f, ComboMultiplier = 1f };
            state = ComboCalculator.TickComboTimer(state, 1f);
            Assert.That(state.ComboCount, Is.EqualTo(0));
            Assert.That(state.ComboTimer, Is.EqualTo(0f));
            Assert.That(state.ComboMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void IncrementCombo_IncrementsCountAndSetsTimer()
        {
            var state = new PlayerState { ComboCount = 0, ComboTimer = 0f, ComboMultiplier = 1f, SpinGauge = 0f, Score = 0 };
            state = ComboCalculator.IncrementCombo(state, 10f, 0.1f, 3f, 0.05f);
            Assert.That(state.ComboCount, Is.EqualTo(1));
            Assert.That(state.ComboTimer, Is.EqualTo(3f));
            Assert.That(state.ComboMultiplier, Is.EqualTo(1.1f).Within(0.01f));
        }

        [Test]
        public void IncrementCombo_AddsAbsorbScore()
        {
            var state = new PlayerState { ComboCount = 0, Score = 100, SpinGauge = 0f, ComboMultiplier = 1f };
            state = ComboCalculator.IncrementCombo(state, 10f, 0.1f, 3f, 0.05f);
            Assert.That(state.Score, Is.EqualTo(111));
        }

        [Test]
        public void IncrementCombo_ClampsSpinGaugeToOne()
        {
            var state = new PlayerState { ComboCount = 0, SpinGauge = 0.98f, Score = 0, ComboMultiplier = 1f };
            state = ComboCalculator.IncrementCombo(state, 10f, 0.1f, 3f, 0.05f);
            Assert.That(state.SpinGauge, Is.EqualTo(1f).Within(0.01f));
        }
    }
}
