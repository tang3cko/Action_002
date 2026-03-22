using NUnit.Framework;
using Action002.Core.Flow;

namespace Action002.Tests.Core
{
    public class GameFlowCommitRunResultTests
    {
        private SpyGameFlowActions spy;
        private GameFlowLogic logic;

        [SetUp]
        public void SetUp()
        {
            spy = new SpyGameFlowActions();
            logic = new GameFlowLogic(spy);
            logic.Initialize();
        }

        #region CommitRunResult called once

        [Test]
        public void HandleGameOver_ShouldCallCommitRunResult()
        {
            logic.HandleGameOver();

            Assert.AreEqual(1, spy.CommitRunResultCallCount);
        }

        [Test]
        public void HandleBossDefeated_ShouldCallCommitRunResult()
        {
            logic.HandleBossDefeated();

            Assert.AreEqual(1, spy.CommitRunResultCallCount);
        }

        #endregion

        #region Double call prevention

        [Test]
        public void HandleGameOver_CalledTwice_ShouldCallCommitRunResultOnce()
        {
            logic.HandleGameOver();
            logic.HandleGameOver();

            Assert.AreEqual(1, spy.CommitRunResultCallCount);
        }

        [Test]
        public void HandleBossDefeated_CalledTwice_ShouldCallCommitRunResultOnce()
        {
            logic.HandleBossDefeated();
            logic.HandleBossDefeated();

            Assert.AreEqual(1, spy.CommitRunResultCallCount);
        }

        [Test]
        public void HandleGameOver_ThenBossDefeated_ShouldCallCommitRunResultOnce()
        {
            logic.HandleGameOver();
            logic.HandleBossDefeated();

            Assert.AreEqual(1, spy.CommitRunResultCallCount);
        }

        #endregion

        #region Flag reset

        [Test]
        public void HandleResultRetrySelected_ShouldResetFlag_AllowingNewCommit()
        {
            logic.HandleGameOver();
            Assert.AreEqual(1, spy.CommitRunResultCallCount);

            logic.HandleResultRetrySelected();
            logic.HandleGameOver();

            Assert.AreEqual(2, spy.CommitRunResultCallCount);
        }

        [Test]
        public void HandleResultBackToTitleSelected_ShouldResetFlag_AllowingNewCommit()
        {
            logic.HandleGameOver();
            Assert.AreEqual(1, spy.CommitRunResultCallCount);

            logic.HandleResultBackToTitleSelected();
            logic.HandleGameOver();

            Assert.AreEqual(2, spy.CommitRunResultCallCount);
        }

        [Test]
        public void HandleBossDefeated_ThenGameOver_ShouldCallCommitRunResultOnce()
        {
            logic.HandleBossDefeated();
            logic.HandleGameOver();

            Assert.AreEqual(1, spy.CommitRunResultCallCount);
        }

        [Test]
        public void HandleResultRetrySelected_ThenBossDefeated_ShouldCallCommitRunResult()
        {
            logic.HandleGameOver();
            Assert.AreEqual(1, spy.CommitRunResultCallCount);

            logic.HandleResultRetrySelected();
            logic.HandleBossDefeated();

            Assert.AreEqual(2, spy.CommitRunResultCallCount);
        }

        [Test]
        public void Initialize_ShouldNotCallCommitRunResult()
        {
            Assert.AreEqual(0, spy.CommitRunResultCallCount);
        }

        [Test]
        public void ThreeRuns_ShouldCommitThreeTimes()
        {
            logic.HandleGameOver();
            logic.HandleResultRetrySelected();
            logic.HandleBossDefeated();
            logic.HandleResultBackToTitleSelected();
            logic.HandleGameOver();

            Assert.AreEqual(3, spy.CommitRunResultCallCount);
        }

        #endregion

        private class SpyGameFlowActions : IGameFlowActions
        {
            public int CommitRunResultCallCount;

            public void LoadScene(string sceneName) { }
            public void CloseTransition() { }
            public void CloseTransitionWithOrigin(float screenX, float screenY) { }
            public void ConvergeTransitionToPlayer() { }
            public void ClearTransitionImmediate() { }
            public void RaiseBossPhaseRequested() { }
            public void RaiseGamePhaseChanged(int phase) { }
            public void SetGamePhaseVariable(int phase) { }
            public void SetResultTypeVariable(int resultType) { }
            public void SaveTutorialCompleted() { }
            public void CommitRunResult() => CommitRunResultCallCount++;
        }
    }
}
