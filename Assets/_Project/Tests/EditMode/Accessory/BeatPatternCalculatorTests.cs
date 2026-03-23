using NUnit.Framework;
using Action002.Accessory.SonicWave.Logic;

namespace Action002.Tests.Accessory
{
    public class BeatPatternCalculatorTests
    {
        [Test]
        public void GetCurrentBeat_HalfBeatIndex0_ReturnsSmallPulse()
        {
            // halfBeatIndex=0 → SmallPulse（とぅん）
            Assert.That(BeatPatternCalculator.GetCurrentBeat(0), Is.EqualTo(SonicWaveBeat.SmallPulse));
        }

        [Test]
        public void GetCurrentBeat_HalfBeatIndex1_ReturnsSmallPulse()
        {
            // halfBeatIndex=1（裏拍）→ SmallPulse（て）
            Assert.That(BeatPatternCalculator.GetCurrentBeat(1), Is.EqualTo(SonicWaveBeat.SmallPulse));
        }

        [Test]
        public void GetCurrentBeat_HalfBeatIndex2_ReturnsLargePulse()
        {
            // halfBeatIndex=2 → LargePulse（てぇん）
            Assert.That(BeatPatternCalculator.GetCurrentBeat(2), Is.EqualTo(SonicWaveBeat.LargePulse));
        }

        [Test]
        public void GetCurrentBeat_HalfBeatIndex3To7_ReturnsRest()
        {
            // halfBeatIndex 3〜7 は全て休符
            for (int i = 3; i <= 7; i++)
            {
                Assert.That(BeatPatternCalculator.GetCurrentBeat(i), Is.EqualTo(SonicWaveBeat.Rest),
                    $"halfBeatIndex={i} should be Rest");
            }
        }

        [Test]
        public void GetCurrentBeat_CyclesAfter8HalfBeats()
        {
            // halfBeatIndex=8 → 8%8=0 → SmallPulse (cycle)
            Assert.That(BeatPatternCalculator.GetCurrentBeat(8), Is.EqualTo(SonicWaveBeat.SmallPulse));
            // halfBeatIndex=9 → 9%8=1 → SmallPulse
            Assert.That(BeatPatternCalculator.GetCurrentBeat(9), Is.EqualTo(SonicWaveBeat.SmallPulse));
            // halfBeatIndex=10 → 10%8=2 → LargePulse
            Assert.That(BeatPatternCalculator.GetCurrentBeat(10), Is.EqualTo(SonicWaveBeat.LargePulse));
        }

        [Test]
        public void IsRestBeat_Rest_ReturnsTrue()
        {
            Assert.That(BeatPatternCalculator.IsRestBeat(SonicWaveBeat.Rest), Is.True);
        }

        [Test]
        public void IsRestBeat_NonRest_ReturnsFalse()
        {
            Assert.That(BeatPatternCalculator.IsRestBeat(SonicWaveBeat.SmallPulse), Is.False);
            Assert.That(BeatPatternCalculator.IsRestBeat(SonicWaveBeat.LargePulse), Is.False);
        }

        [Test]
        public void GetCurrentBeat_LargeHalfBeatIndex_StillCyclesCorrectly()
        {
            // halfBeatIndex=100 → 100%8=4 → Rest
            Assert.That(BeatPatternCalculator.GetCurrentBeat(100), Is.EqualTo(SonicWaveBeat.Rest));
            // halfBeatIndex=97 → 97%8=1 → SmallPulse
            Assert.That(BeatPatternCalculator.GetCurrentBeat(97), Is.EqualTo(SonicWaveBeat.SmallPulse));
        }

        [Test]
        public void PatternLength_Is8()
        {
            Assert.That(BeatPatternCalculator.PATTERN_LENGTH, Is.EqualTo(8));
        }
    }
}
