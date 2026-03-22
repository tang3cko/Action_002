using NUnit.Framework;
using Action002.Core.Save;

namespace Action002.Tests.Core
{
    public class RunSessionStatsCalculatorTests
    {
        private RunSessionStatsCalculator calculator;

        [SetUp]
        public void SetUp()
        {
            calculator = new RunSessionStatsCalculator();
        }

        #region Initial State

        [Test]
        public void InitialState_AllCountsShouldBeZero()
        {
            Assert.AreEqual(0, calculator.MaxCombo);
            Assert.AreEqual(0, calculator.KillCount);
            Assert.AreEqual(0, calculator.AbsorptionCount);
        }

        #endregion

        #region RecordKill

        [Test]
        public void RecordKill_ShouldIncrementKillCount()
        {
            calculator.RecordKill();

            Assert.AreEqual(1, calculator.KillCount);
        }

        [Test]
        public void RecordKill_CalledMultipleTimes_ShouldAccumulate()
        {
            calculator.RecordKill();
            calculator.RecordKill();
            calculator.RecordKill();

            Assert.AreEqual(3, calculator.KillCount);
        }

        [Test]
        public void RecordKill_ShouldNotAffectOtherCounters()
        {
            calculator.RecordKill();

            Assert.AreEqual(0, calculator.MaxCombo);
            Assert.AreEqual(0, calculator.AbsorptionCount);
        }

        #endregion

        #region RecordAbsorption

        [Test]
        public void RecordAbsorption_ShouldIncrementAbsorptionCount()
        {
            calculator.RecordAbsorption(1);

            Assert.AreEqual(1, calculator.AbsorptionCount);
        }

        [Test]
        public void RecordAbsorption_ShouldUpdateMaxComboIfHigher()
        {
            calculator.RecordAbsorption(5);

            Assert.AreEqual(5, calculator.MaxCombo);
        }

        [Test]
        public void RecordAbsorption_ShouldNotUpdateMaxComboIfLower()
        {
            calculator.RecordAbsorption(10);
            calculator.RecordAbsorption(3);

            Assert.AreEqual(10, calculator.MaxCombo);
        }

        [Test]
        public void RecordAbsorption_EqualCombo_ShouldNotChangeMaxCombo()
        {
            calculator.RecordAbsorption(7);
            calculator.RecordAbsorption(7);

            Assert.AreEqual(7, calculator.MaxCombo);
        }

        [Test]
        public void RecordAbsorption_ComboZero_ShouldNotUpdateMaxCombo()
        {
            calculator.RecordAbsorption(5);
            calculator.RecordAbsorption(0);

            Assert.AreEqual(5, calculator.MaxCombo);
        }

        [Test]
        public void RecordAbsorption_CalledMultipleTimes_ShouldAccumulateCount()
        {
            calculator.RecordAbsorption(1);
            calculator.RecordAbsorption(2);
            calculator.RecordAbsorption(3);

            Assert.AreEqual(3, calculator.AbsorptionCount);
        }

        [Test]
        public void RecordAbsorption_ShouldTrackPeakComboAcrossMultipleCalls()
        {
            calculator.RecordAbsorption(3);
            calculator.RecordAbsorption(10);
            calculator.RecordAbsorption(7);

            Assert.AreEqual(10, calculator.MaxCombo);
        }

        [Test]
        public void RecordAbsorption_ShouldNotAffectKillCount()
        {
            calculator.RecordAbsorption(5);

            Assert.AreEqual(0, calculator.KillCount);
        }

        #endregion

        #region Reset

        [Test]
        public void Reset_ShouldClearAllCounters()
        {
            calculator.RecordKill();
            calculator.RecordAbsorption(10);

            calculator.Reset();

            Assert.AreEqual(0, calculator.MaxCombo);
            Assert.AreEqual(0, calculator.KillCount);
            Assert.AreEqual(0, calculator.AbsorptionCount);
        }

        [Test]
        public void Reset_AfterReset_ShouldAccumulateFromZero()
        {
            calculator.RecordKill();
            calculator.RecordKill();
            calculator.Reset();
            calculator.RecordKill();

            Assert.AreEqual(1, calculator.KillCount);
        }

        #endregion
    }
}
