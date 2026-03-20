using Action002.Core.Flow;
using NUnit.Framework;

namespace Action002.Tests.Core
{
    public class SceneTransitionTrackerTests
    {
        private SceneTransitionTracker tracker;

        [SetUp]
        public void SetUp()
        {
            tracker = new SceneTransitionTracker();
        }

        #region TryBeginTransition

        [Test]
        public void TryBeginTransition_NotTransitioning_ReturnsTrue()
        {
            // Arrange & Act
            bool result = tracker.TryBeginTransition("Gameplay");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(tracker.IsTransitioning, Is.True);
        }

        [Test]
        public void TryBeginTransition_AlreadyTransitioning_ReturnsFalse()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");

            // Act
            bool result = tracker.TryBeginTransition("Result");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryBeginTransition_AfterEndTransition_ReturnsTrue()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");
            tracker.EndTransition();

            // Act
            bool result = tracker.TryBeginTransition("Result");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(tracker.IsTransitioning, Is.True);
        }

        #endregion

        #region SetLoadedScene

        [Test]
        public void SetLoadedScene_UpdatesName()
        {
            // Arrange & Act
            tracker.SetLoadedScene("Gameplay");

            // Assert
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
        }

        [Test]
        public void SetLoadedScene_CalledTwice_OverwritesPreviousValue()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");

            // Act
            tracker.SetLoadedScene("Result");

            // Assert
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));
        }

        #endregion

        #region HasLoadedScene

        [Test]
        public void HasLoadedScene_WithScene_ReturnsTrue()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");

            // Act & Assert
            Assert.That(tracker.HasLoadedScene(), Is.True);
        }

        [Test]
        public void HasLoadedScene_Empty_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.That(tracker.HasLoadedScene(), Is.False);
        }

        [Test]
        public void HasLoadedScene_AfterSetNull_ReturnsFalse()
        {
            // Arrange
            tracker.SetLoadedScene(null);

            // Act & Assert
            Assert.That(tracker.HasLoadedScene(), Is.False);
        }

        [Test]
        public void HasLoadedScene_AfterSetEmptyString_ReturnsFalse()
        {
            // Arrange
            tracker.SetLoadedScene("");

            // Act & Assert
            Assert.That(tracker.HasLoadedScene(), Is.False);
        }

        #endregion

        #region ConsumePendingLoad

        [Test]
        public void ConsumePendingLoad_ReturnsPendingAndClears()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");

            // Act
            string pending = tracker.ConsumePendingLoad();

            // Assert
            Assert.That(pending, Is.EqualTo("Gameplay"));
            Assert.That(tracker.ConsumePendingLoad(), Is.Null);
        }

        [Test]
        public void ConsumePendingLoad_WhenNeverSet_ReturnsNull()
        {
            // Arrange & Act
            string result = tracker.ConsumePendingLoad();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConsumePendingLoad_CalledTwice_SecondReturnsNull()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");
            tracker.ConsumePendingLoad();

            // Act
            string result = tracker.ConsumePendingLoad();

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region BeginUnloadCurrent

        [Test]
        public void BeginUnloadCurrent_ClearsLoadedScene()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");

            // Act
            tracker.BeginUnloadCurrent();

            // Assert
            Assert.That(tracker.LoadedContentSceneName, Is.Null);
            Assert.That(tracker.HasLoadedScene(), Is.False);
        }

        [Test]
        public void BeginUnloadCurrent_SetsUnloadingSceneName()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");

            // Act
            tracker.BeginUnloadCurrent();

            // Assert
            Assert.That(tracker.ShouldHandleUnloadEvent("Gameplay"), Is.True);
        }

        #endregion

        #region ShouldHandleUnloadEvent

        [Test]
        public void ShouldHandleUnloadEvent_MatchingScene_ReturnsTrue()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");
            tracker.BeginUnloadCurrent();

            // Act
            bool result = tracker.ShouldHandleUnloadEvent("Gameplay");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldHandleUnloadEvent_DifferentScene_ReturnsFalse()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");
            tracker.BeginUnloadCurrent();

            // Act
            bool result = tracker.ShouldHandleUnloadEvent("Result");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldHandleUnloadEvent_NoUnloading_ReturnsFalse()
        {
            // Arrange & Act
            bool result = tracker.ShouldHandleUnloadEvent("Gameplay");

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region HandleUnloadCompleted

        [Test]
        public void HandleUnloadCompleted_ClearsUnloading()
        {
            // Arrange
            tracker.SetLoadedScene("Gameplay");
            tracker.BeginUnloadCurrent();

            // Act
            string unloaded = tracker.HandleUnloadCompleted();

            // Assert
            Assert.That(unloaded, Is.EqualTo("Gameplay"));
            Assert.That(tracker.ShouldHandleUnloadEvent("Gameplay"), Is.False);
        }

        [Test]
        public void HandleUnloadCompleted_WhenNoUnloading_ReturnsNull()
        {
            // Arrange & Act
            string result = tracker.HandleUnloadCompleted();

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region ShouldHandleLoadEvent

        [Test]
        public void ShouldHandleLoadEvent_MatchesPending_ReturnsTrue()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");

            // Act
            bool result = tracker.ShouldHandleLoadEvent("Gameplay");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldHandleLoadEvent_DifferentFromPending_ReturnsFalse()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");

            // Act
            bool result = tracker.ShouldHandleLoadEvent("Result");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldHandleLoadEvent_NoPendingAndNotTransitioning_ReturnsFalse()
        {
            // Arrange - no pending, not transitioning

            // Act
            bool result = tracker.ShouldHandleLoadEvent("AnyScene");

            // Assert - not transitioning means we should not handle any load event
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldHandleLoadEvent_NoPendingButTransitioning_ReturnsFalse()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");
            tracker.ConsumePendingLoad(); // clear pending but still transitioning

            // Act
            bool result = tracker.ShouldHandleLoadEvent("AnyScene");

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region HandleLoadCompleted

        [Test]
        public void HandleLoadCompleted_SetsLoadedScene()
        {
            // Arrange & Act
            tracker.HandleLoadCompleted("Gameplay");

            // Assert
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
            Assert.That(tracker.HasLoadedScene(), Is.True);
        }

        [Test]
        public void HandleLoadCompleted_ClearsPendingScene()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");

            // Act
            tracker.HandleLoadCompleted("Gameplay");

            // Assert
            Assert.That(tracker.ConsumePendingLoad(), Is.Null);
        }

        #endregion

        #region EndTransition

        [Test]
        public void EndTransition_ClearsAllState()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");
            tracker.SetLoadedScene("OldScene");
            tracker.BeginUnloadCurrent();

            // Act
            tracker.EndTransition();

            // Assert
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.ShouldHandleUnloadEvent("OldScene"), Is.False);
            Assert.That(tracker.ConsumePendingLoad(), Is.Null);
        }

        [Test]
        public void EndTransition_WhenNotTransitioning_RemainsIdle()
        {
            // Arrange & Act
            tracker.EndTransition();

            // Assert
            Assert.That(tracker.IsTransitioning, Is.False);
        }

        #endregion

        #region AbortTransition

        [Test]
        public void AbortTransition_ClearsAllState()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");
            tracker.SetLoadedScene("OldScene");
            tracker.BeginUnloadCurrent();

            // Act
            tracker.AbortTransition();

            // Assert
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.ShouldHandleUnloadEvent("OldScene"), Is.False);
            Assert.That(tracker.ConsumePendingLoad(), Is.Null);
        }

        [Test]
        public void AbortTransition_AllowsNewTransition()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");

            // Act
            tracker.AbortTransition();
            bool result = tracker.TryBeginTransition("Result");

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region Scenario: Title → Gameplay

        [Test]
        public void Scenario_TitleToGameplay_NoLoadedScene_LoadDirectly()
        {
            // Arrange - initial state, no loaded scene

            // Assert initial state
            Assert.That(tracker.HasLoadedScene(), Is.False);
            Assert.That(tracker.IsTransitioning, Is.False);

            // Act - begin transition to Gameplay
            bool began = tracker.TryBeginTransition("Gameplay");
            Assert.That(began, Is.True);
            Assert.That(tracker.IsTransitioning, Is.True);

            // No current scene to unload, so load directly
            Assert.That(tracker.HasLoadedScene(), Is.False);

            // Consume pending to get the target scene
            string pending = tracker.ConsumePendingLoad();
            Assert.That(pending, Is.EqualTo("Gameplay"));

            // Load completed
            tracker.HandleLoadCompleted("Gameplay");
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
            Assert.That(tracker.HasLoadedScene(), Is.True);

            // End transition
            tracker.EndTransition();
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
        }

        #endregion

        #region Scenario: Gameplay → Result

        [Test]
        public void Scenario_GameplayToResult_UnloadThenLoad()
        {
            // Arrange - tracker has Gameplay loaded
            tracker.SetLoadedScene("Gameplay");
            Assert.That(tracker.HasLoadedScene(), Is.True);

            // Act - begin transition to Result
            bool began = tracker.TryBeginTransition("Result");
            Assert.That(began, Is.True);

            // Begin unload of current scene
            tracker.BeginUnloadCurrent();
            Assert.That(tracker.LoadedContentSceneName, Is.Null);
            Assert.That(tracker.ShouldHandleUnloadEvent("Gameplay"), Is.True);

            // Handle unload completed
            string unloaded = tracker.HandleUnloadCompleted();
            Assert.That(unloaded, Is.EqualTo("Gameplay"));
            Assert.That(tracker.ShouldHandleUnloadEvent("Gameplay"), Is.False);

            // Load Result scene
            tracker.HandleLoadCompleted("Result");
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));
            Assert.That(tracker.HasLoadedScene(), Is.True);

            // End transition
            tracker.EndTransition();
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));
        }

        #endregion

        #region Scenario: Result → Title

        [Test]
        public void Scenario_ResultToTitle_UnloadThenLoad()
        {
            // Arrange - tracker has Result loaded
            tracker.SetLoadedScene("Result");

            // Act - begin transition to Title
            bool began = tracker.TryBeginTransition("Title");
            Assert.That(began, Is.True);

            // Unload Result
            tracker.BeginUnloadCurrent();
            Assert.That(tracker.ShouldHandleUnloadEvent("Result"), Is.True);

            string unloaded = tracker.HandleUnloadCompleted();
            Assert.That(unloaded, Is.EqualTo("Result"));

            // Load Title
            tracker.HandleLoadCompleted("Title");
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Title"));

            // End transition
            tracker.EndTransition();
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Title"));
        }

        #endregion

        #region Scenario: Result → Gameplay (Retry)

        [Test]
        public void Scenario_ResultToGameplay_Retry()
        {
            // Arrange - tracker has Result loaded
            tracker.SetLoadedScene("Result");

            // Act - begin transition to Gameplay (retry)
            bool began = tracker.TryBeginTransition("Gameplay");
            Assert.That(began, Is.True);

            // Unload Result
            tracker.BeginUnloadCurrent();
            Assert.That(tracker.ShouldHandleUnloadEvent("Result"), Is.True);

            string unloaded = tracker.HandleUnloadCompleted();
            Assert.That(unloaded, Is.EqualTo("Result"));

            // Load Gameplay
            tracker.HandleLoadCompleted("Gameplay");
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));

            // End transition
            tracker.EndTransition();
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
        }

        #endregion

        #region Scenario: Double Transition Blocked

        [Test]
        public void Scenario_DoubleTransitionBlocked()
        {
            // Arrange - begin first transition
            tracker.TryBeginTransition("Gameplay");

            // Act - attempt second transition while first is active
            bool secondResult = tracker.TryBeginTransition("Result");

            // Assert
            Assert.That(secondResult, Is.False);
            Assert.That(tracker.IsTransitioning, Is.True);

            // After completing first transition, second is allowed
            tracker.EndTransition();
            bool thirdResult = tracker.TryBeginTransition("Result");
            Assert.That(thirdResult, Is.True);
        }

        #endregion

        #region Scenario: Abort Mid-Transition

        [Test]
        public void Scenario_AbortMidTransition_CleansUpState()
        {
            // Arrange - start a transition with unload in progress
            tracker.SetLoadedScene("Gameplay");
            tracker.TryBeginTransition("Result");
            tracker.BeginUnloadCurrent();

            // Act - abort
            tracker.AbortTransition();

            // Assert - state is cleaned up
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.ConsumePendingLoad(), Is.Null);
            Assert.That(tracker.ShouldHandleUnloadEvent("Gameplay"), Is.False);
        }

        [Test]
        public void Scenario_AbortThenNewTransition_Succeeds()
        {
            // Arrange
            tracker.TryBeginTransition("Gameplay");
            tracker.AbortTransition();

            // Act
            bool result = tracker.TryBeginTransition("Title");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(tracker.IsTransitioning, Is.True);
        }

        #endregion

        #region Scenario: Full Lifecycle Title → Gameplay → Result → Title

        [Test]
        public void Scenario_FullLifecycle_TitleToGameplayToResultToTitle()
        {
            // --- Title → Gameplay (no scene to unload) ---
            Assert.That(tracker.TryBeginTransition("Gameplay"), Is.True);
            Assert.That(tracker.HasLoadedScene(), Is.False);
            tracker.HandleLoadCompleted("Gameplay");
            tracker.EndTransition();

            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));

            // --- Gameplay → Result ---
            Assert.That(tracker.TryBeginTransition("Result"), Is.True);
            tracker.BeginUnloadCurrent();
            Assert.That(tracker.ShouldHandleUnloadEvent("Gameplay"), Is.True);
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Result");
            tracker.EndTransition();

            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));

            // --- Result → Title ---
            Assert.That(tracker.TryBeginTransition("Title"), Is.True);
            tracker.BeginUnloadCurrent();
            Assert.That(tracker.ShouldHandleUnloadEvent("Result"), Is.True);
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Title");
            tracker.EndTransition();

            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Title"));
        }

        #endregion
    }
}
