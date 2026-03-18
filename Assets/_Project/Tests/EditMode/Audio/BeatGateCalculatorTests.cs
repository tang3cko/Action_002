using NUnit.Framework;
using Action002.Audio.Logic;

namespace Action002.Tests.Audio
{
    public class BeatGateCalculatorTests
    {
        [Test]
        public void ShouldFire_NewBeat_ReturnsTrue()
        {
            Assert.That(BeatGateCalculator.ShouldFire(1, 0), Is.True);
        }

        [Test]
        public void ShouldFire_SameBeat_ReturnsFalse()
        {
            Assert.That(BeatGateCalculator.ShouldFire(0, 0), Is.False);
        }

        [Test]
        public void ShouldFire_SkippedBeats_ReturnsTrue()
        {
            Assert.That(BeatGateCalculator.ShouldFire(3, 0), Is.True);
        }

        [Test]
        public void ShouldFireOnDownbeat_EvenIndex_ReturnsTrue()
        {
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(2, 1), Is.True);
        }

        [Test]
        public void ShouldFireOnDownbeat_OddIndex_ReturnsFalse()
        {
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(3, 2), Is.False);
        }

        [Test]
        public void ShouldFireOnOffbeat_OddIndex_ReturnsTrue()
        {
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(3, 2), Is.True);
        }

        [Test]
        public void ShouldFireOnOffbeat_EvenIndex_ReturnsFalse()
        {
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(2, 1), Is.False);
        }

        // --- Consecutive call (consume-then-recheck) tests ---

        [Test]
        public void ShouldFireOnDownbeat_ConsecutiveCalls_ConsumedIndexPreventsRefiring()
        {
            // Frame 1: index advances to 2 (downbeat), lastConsumed=1 → fires
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(2, 1), Is.True);

            // Caller consumes: lastConsumed becomes 2
            // Frame 2: index still 2, lastConsumed=2 → should NOT fire again
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(2, 2), Is.False);
        }

        [Test]
        public void ShouldFireOnOffbeat_ConsecutiveCalls_ConsumedIndexPreventsRefiring()
        {
            // Frame 1: index advances to 3 (offbeat), lastConsumed=2 → fires
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(3, 2), Is.True);

            // Caller consumes: lastConsumed becomes 3
            // Frame 2: index still 3, lastConsumed=3 → should NOT fire again
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(3, 3), Is.False);
        }

        // --- Frame skip scenarios ---

        [Test]
        public void ShouldFireOnDownbeat_FrameSkip_LastConsumed0_Current5_ReturnsTrue()
        {
            // Skipped several beats. Current index 5 is odd (offbeat),
            // but ShouldFireOnDownbeat checks the CURRENT index only.
            // Index 5 is odd → not a downbeat → false.
            // However, the spec says "index 2 or 4 are downbeats" implying
            // the method should return true because we skipped past downbeats.
            // Actual implementation: checks currentHalfBeatIndex only.
            // current=5 is odd → false.
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(5, 0), Is.False);
        }

        [Test]
        public void ShouldFireOnDownbeat_FrameSkip_LastConsumed0_Current4_ReturnsTrue()
        {
            // Skipped beats, current=4 (even=downbeat), lastConsumed=0 → fires
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(4, 0), Is.True);
        }

        [Test]
        public void ShouldFireOnOffbeat_FrameSkip_LastConsumed0_Current4_ReturnsFalse()
        {
            // current=4 is even (downbeat), so offbeat check is false
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(4, 0), Is.False);
        }

        [Test]
        public void ShouldFireOnOffbeat_FrameSkip_LastConsumed0_Current5_ReturnsTrue()
        {
            // current=5 is odd (offbeat), lastConsumed=0 → new beat → fires
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(5, 0), Is.True);
        }

        // --- Rapid toggling: fire, consume, advance 1, fire again ---

        [Test]
        public void ShouldFire_RapidToggle_FireConsumeAdvanceFire()
        {
            int lastConsumed = 1;

            // Beat advances to 2 → fire
            Assert.That(BeatGateCalculator.ShouldFire(2, lastConsumed), Is.True);
            lastConsumed = 2;

            // Same frame, no advance → no fire
            Assert.That(BeatGateCalculator.ShouldFire(2, lastConsumed), Is.False);

            // Advance by 1 to 3 → fire again
            Assert.That(BeatGateCalculator.ShouldFire(3, lastConsumed), Is.True);
            lastConsumed = 3;

            // Consumed → no fire
            Assert.That(BeatGateCalculator.ShouldFire(3, lastConsumed), Is.False);
        }

        // --- Negative lastConsumedIndex (initial state) tests ---

        [Test]
        public void ShouldFire_NegativeLastConsumed_ReturnsTrue()
        {
            // Initial state: lastConsumed=-1, current=0 → should fire
            Assert.That(BeatGateCalculator.ShouldFire(0, -1), Is.True);
        }

        [Test]
        public void ShouldFireOnDownbeat_NegativeLastConsumed_Current0_ReturnsTrue()
        {
            // First beat (index 0) is even → downbeat, lastConsumed=-1 → fires
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(0, -1), Is.True);
        }

        [Test]
        public void ShouldFireOnOffbeat_NegativeLastConsumed_Current0_ReturnsFalse()
        {
            // First beat (index 0) is even → downbeat, NOT offbeat
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(0, -1), Is.False);
        }

        [Test]
        public void ShouldFireOnOffbeat_NegativeLastConsumed_Current1_ReturnsTrue()
        {
            // Index 1 is odd → offbeat, lastConsumed=-1 → fires
            Assert.That(BeatGateCalculator.ShouldFireOnOffbeat(1, -1), Is.True);
        }

        [Test]
        public void ShouldFireOnDownbeat_NegativeLastConsumed_Current1_ReturnsFalse()
        {
            // Index 1 is odd → offbeat, not downbeat
            Assert.That(BeatGateCalculator.ShouldFireOnDownbeat(1, -1), Is.False);
        }
    }
}
