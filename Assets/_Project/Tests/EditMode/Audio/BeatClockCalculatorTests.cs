using NUnit.Framework;
using Action002.Audio.Logic;

namespace Action002.Tests.Audio
{
    public class BeatClockCalculatorTests
    {
        [Test]
        public void GetHalfBeatIndex_AtZero_ReturnsZero()
        {
            int result = BeatClockCalculator.GetHalfBeatIndex(0.0, 0.25f);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetHalfBeatIndex_BeforeBoundary_ReturnsZero()
        {
            int result = BeatClockCalculator.GetHalfBeatIndex(0.24, 0.25f);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetHalfBeatIndex_AtFirstHalfBeat_ReturnsOne()
        {
            int result = BeatClockCalculator.GetHalfBeatIndex(0.25, 0.25f);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void GetHalfBeatIndex_AtSecondHalfBeat_ReturnsTwo()
        {
            int result = BeatClockCalculator.GetHalfBeatIndex(0.50, 0.25f);
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void GetHalfBeatIndex_AtOneSecond_ReturnsFour()
        {
            int result = BeatClockCalculator.GetHalfBeatIndex(1.0, 0.25f);
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public void IsDownbeat_EvenIndex_ReturnsTrue()
        {
            Assert.That(BeatClockCalculator.IsDownbeat(0), Is.True);
            Assert.That(BeatClockCalculator.IsDownbeat(2), Is.True);
        }

        [Test]
        public void IsDownbeat_OddIndex_ReturnsFalse()
        {
            Assert.That(BeatClockCalculator.IsDownbeat(1), Is.False);
        }

        [Test]
        public void SecondsPerHalfBeat_120BPM_ReturnsQuarter()
        {
            float result = BeatClockCalculator.SecondsPerHalfBeat(120f);
            Assert.That(result, Is.EqualTo(0.25f));
        }

        [Test]
        public void SecondsPerHalfBeat_ZeroBPM_ReturnsSafeDefault()
        {
            float result = BeatClockCalculator.SecondsPerHalfBeat(0f);
            Assert.That(result, Is.EqualTo(0.25f));
        }

        [Test]
        public void GetHalfBeatIndex_ZeroSecondsPerHalfBeat_ReturnsZero()
        {
            int result = BeatClockCalculator.GetHalfBeatIndex(1.0, 0f);
            Assert.That(result, Is.EqualTo(0));
        }
    }
}
