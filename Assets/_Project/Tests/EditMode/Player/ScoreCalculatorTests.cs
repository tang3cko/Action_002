using NUnit.Framework;
using Action002.Player.Data;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class ScoreCalculatorTests
    {
        [Test]
        public void AddKillScore_AddsBaseScore()
        {
            var state = new PlayerState { Score = 100, SpinGauge = 0f };
            state = ScoreCalculator.AddKillScore(state, 50, 0.05f);
            Assert.That(state.Score, Is.EqualTo(150));
        }

        [Test]
        public void AddKillScore_AddsGauge()
        {
            var state = new PlayerState { Score = 0, SpinGauge = 0.5f };
            state = ScoreCalculator.AddKillScore(state, 10, 0.1f);
            Assert.That(state.SpinGauge, Is.EqualTo(0.6f).Within(0.01f));
        }

        [Test]
        public void AddKillScore_ClampsGaugeToOne()
        {
            var state = new PlayerState { Score = 0, SpinGauge = 0.95f };
            state = ScoreCalculator.AddKillScore(state, 10, 0.1f);
            Assert.That(state.SpinGauge, Is.EqualTo(1f).Within(0.01f));
        }
    }
}
