using NUnit.Framework;
using Action002.Player.Data;
using Action002.Player.Logic;

namespace Action002.Tests.Player
{
    public class PlayerGrowthCoordinatorTests
    {
        private MockGrowthActions mockActions;
        private PlayerGrowthCoordinator coordinator;

        [SetUp]
        public void SetUp()
        {
            mockActions = new MockGrowthActions();
            coordinator = new PlayerGrowthCoordinator(mockActions);
        }

        // ── CheckAndApplyGrowth ──

        [Test]
        public void CheckAndApplyGrowth_GaugeBelowThreshold_DoesNothing()
        {
            coordinator.CheckAndApplyGrowth(0.5f);

            Assert.That(mockActions.ResetSpinGaugeCalled, Is.False);
            Assert.That(mockActions.ApplyGrowthCalled, Is.False);
            Assert.That(mockActions.RaiseLevelUpCalled, Is.False);
        }

        [Test]
        public void CheckAndApplyGrowth_GaugeAtThreshold_TriggersLevelUp()
        {
            coordinator.CheckAndApplyGrowth(1.0f);

            Assert.That(mockActions.ResetSpinGaugeCalled, Is.True);
            Assert.That(mockActions.ApplyGrowthCalled, Is.True);
            Assert.That(mockActions.RaiseLevelUpCalled, Is.True);
            Assert.That(mockActions.LastLevel, Is.EqualTo(1));
        }

        [Test]
        public void CheckAndApplyGrowth_GaugeAtThreshold_ResetsGaugeBeforeLevelUp()
        {
            coordinator.CheckAndApplyGrowth(1.0f);

            // ResetSpinGauge should be called (order verified by call index)
            Assert.That(mockActions.ResetSpinGaugeCallIndex, Is.LessThan(mockActions.ApplyGrowthCallIndex));
        }

        [Test]
        public void CheckAndApplyGrowth_GaugeAtThreshold_AppliesGrowthState()
        {
            coordinator.CheckAndApplyGrowth(1.0f);

            Assert.That(mockActions.LastGrowthState.Level, Is.EqualTo(1));
            Assert.That(mockActions.LastGrowthState.BulletCount, Is.EqualTo(2));
        }

        [Test]
        public void CheckAndApplyGrowth_CalledTwiceWithThreshold_ProgressesToLevel2()
        {
            coordinator.CheckAndApplyGrowth(1.0f);
            coordinator.CheckAndApplyGrowth(1.0f);

            Assert.That(mockActions.LastLevel, Is.EqualTo(2));
            Assert.That(coordinator.CurrentState.Level, Is.EqualTo(2));
        }

        // ── Reset ──

        [Test]
        public void Reset_ResetsGrowthStateToDefault()
        {
            coordinator.CheckAndApplyGrowth(1.0f);
            coordinator.Reset();

            Assert.That(coordinator.CurrentState.Level, Is.EqualTo(0));
            Assert.That(coordinator.CurrentState.BulletCount, Is.EqualTo(1));
            Assert.That(coordinator.CurrentState.MoveSpeedMultiplier, Is.EqualTo(1f));
            Assert.That(coordinator.CurrentState.BulletSpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Reset_ThenLevelUp_StartsFromLevel1Again()
        {
            coordinator.CheckAndApplyGrowth(1.0f); // Level 1
            coordinator.CheckAndApplyGrowth(1.0f); // Level 2
            coordinator.Reset();
            coordinator.CheckAndApplyGrowth(1.0f); // Level 1 again

            Assert.That(coordinator.CurrentState.Level, Is.EqualTo(1));
            Assert.That(mockActions.LastLevel, Is.EqualTo(1));
        }

        // ── CurrentState ──

        [Test]
        public void CurrentState_InitiallyDefault()
        {
            var state = coordinator.CurrentState;
            Assert.That(state.Level, Is.EqualTo(0));
            Assert.That(state.BulletCount, Is.EqualTo(1));
        }

        // ── Helper ──

        private class MockGrowthActions : IPlayerGrowthActions
        {
            public bool ResetSpinGaugeCalled;
            public bool ApplyGrowthCalled;
            public bool RaiseLevelUpCalled;
            public int LastLevel;
            public PlayerGrowthState LastGrowthState;
            public int ResetSpinGaugeCallIndex;
            public int ApplyGrowthCallIndex;
            private int callCounter;

            public void ResetSpinGauge()
            {
                ResetSpinGaugeCalled = true;
                ResetSpinGaugeCallIndex = callCounter++;
            }

            public void ApplyGrowth(PlayerGrowthState state)
            {
                ApplyGrowthCalled = true;
                LastGrowthState = state;
                ApplyGrowthCallIndex = callCounter++;
            }

            public void RaiseLevelUp(int level)
            {
                RaiseLevelUpCalled = true;
                LastLevel = level;
            }
        }
    }
}
