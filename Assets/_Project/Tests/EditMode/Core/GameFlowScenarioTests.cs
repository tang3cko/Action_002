using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Action002.Core.Flow;
using Action002.Core.Save;
using Action002.Enemy.Data;
using Action002.Visual;
using Tang3cko.ReactiveSO;
using Unity.Mathematics;
using UnityEngine.TestTools;

// Note: System.Reflection is still used by non-Controller tests (SetField/GetField for SO wiring).

namespace Action002.Tests.Core
{
    /// <summary>
    /// Scenario tests that verify EventChannel chains, VariableSO state changes,
    /// and GameFlowController behavior driving the game flow.
    /// </summary>
    public class GameFlowScenarioTests
    {
        private readonly List<Object> disposables = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                    Object.DestroyImmediate(disposables[i]);
            }
            disposables.Clear();
        }

        private T CreateSO<T>() where T : ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<T>();
            disposables.Add(so);
            return so;
        }

        /// <summary>
        /// Sets a private serialized field on a ScriptableObject via reflection.
        /// </summary>
        private static void SetField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Assert.Fail($"Field '{fieldName}' not found on {target.GetType().Name} or its base types.");
        }

        #region Phase Change Events

        [Test]
        public void TitleStart_ShouldNotImmediatelyChangePhase()
        {
            // Arrange
            var onTitleStartSelected = CreateSO<VoidEventChannelSO>();
            var gamePhaseVar = CreateSO<IntVariableSO>();
            gamePhaseVar.Value = (int)GamePhase.Title;

            bool valueChanged = false;
            var onValueChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onValueChanged);
            onValueChanged.OnEventRaised += _ => valueChanged = true;

            // Act - fire the title start event (no controller to handle it)
            onTitleStartSelected.RaiseEvent();

            // Assert - gamePhaseVar should NOT have changed to Stage
            Assert.That(valueChanged, Is.False,
                "Phase should not change immediately on title start; transition must complete first.");
            Assert.That(gamePhaseVar.Value, Is.EqualTo((int)GamePhase.Title));
        }

        [Test]
        public void GamePhaseChanged_ToTitle_ShouldFireEvent()
        {
            // Arrange
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);

            int receivedPhase = -1;
            onGamePhaseChanged.OnEventRaised += value => receivedPhase = value;

            // Act
            gamePhaseVar.Value = (int)GamePhase.Title;

            // Assert
            Assert.That(receivedPhase, Is.EqualTo((int)GamePhase.Title));
        }

        [Test]
        public void GamePhaseChanged_ToStage_ShouldFireEvent()
        {
            // Arrange
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);

            int receivedPhase = -1;
            onGamePhaseChanged.OnEventRaised += value => receivedPhase = value;

            // Act
            gamePhaseVar.Value = (int)GamePhase.Stage;

            // Assert
            Assert.That(receivedPhase, Is.EqualTo((int)GamePhase.Stage));
        }

        [Test]
        public void GamePhaseChanged_ToResult_ShouldFireEvent()
        {
            // Arrange
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);

            int receivedPhase = -1;
            onGamePhaseChanged.OnEventRaised += value => receivedPhase = value;

            // Act
            gamePhaseVar.Value = (int)GamePhase.Result;

            // Assert
            Assert.That(receivedPhase, Is.EqualTo((int)GamePhase.Result));
        }

        [Test]
        public void GamePhaseChanged_ToBoss_ShouldFireEvent()
        {
            // Arrange
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);

            int receivedPhase = -1;
            onGamePhaseChanged.OnEventRaised += value => receivedPhase = value;

            // Act
            gamePhaseVar.Value = (int)GamePhase.Boss;

            // Assert
            Assert.That(receivedPhase, Is.EqualTo((int)GamePhase.Boss));
        }

        [Test]
        public void GamePhaseChanged_SameValue_ShouldNotFireEvent()
        {
            // Arrange
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);

            gamePhaseVar.Value = (int)GamePhase.Title;

            int fireCount = 0;
            onGamePhaseChanged.OnEventRaised += _ => fireCount++;

            // Act - set to the same value again
            gamePhaseVar.Value = (int)GamePhase.Title;

            // Assert
            Assert.That(fireCount, Is.EqualTo(0),
                "VariableSO should not fire event when value does not change.");
        }

        #endregion

        #region Result Type

        [Test]
        public void ResultType_SetBeforePhaseChange_ShouldBeReadable()
        {
            // Arrange
            var resultTypeVar = CreateSO<IntVariableSO>();
            var gamePhaseVar = CreateSO<IntVariableSO>();

            // Act - set result type before phase change
            resultTypeVar.Value = (int)GameResultType.GameOver;

            // Assert - result type is correct
            Assert.That(resultTypeVar.Value, Is.EqualTo((int)GameResultType.GameOver));

            // Act - change to result phase
            gamePhaseVar.Value = (int)GamePhase.Result;

            // Assert - result type is still correct
            Assert.That(resultTypeVar.Value, Is.EqualTo((int)GameResultType.GameOver));
        }

        [Test]
        public void ResultType_Clear_ShouldBeReadable()
        {
            // Arrange
            var resultTypeVar = CreateSO<IntVariableSO>();

            // Act
            resultTypeVar.Value = (int)GameResultType.Clear;

            // Assert
            Assert.That(resultTypeVar.Value, Is.EqualTo((int)GameResultType.Clear));
        }

        #endregion

        #region Variable Reset

        [Test]
        public void PlayerHpVar_ResetToInitial_ShouldRestoreMaxHp()
        {
            // Arrange
            var playerHpVar = CreateSO<IntVariableSO>();
            SetField(playerHpVar, "initialValue", 5);
            playerHpVar.Value = 5; // sync to initial
            playerHpVar.Value = 2; // simulate damage

            // Act
            playerHpVar.ResetToInitial();

            // Assert
            Assert.That(playerHpVar.Value, Is.EqualTo(5));
        }

        [Test]
        public void PlayerHpVar_ResetToInitial_ShouldFireValueChangedEvent()
        {
            // Arrange
            var playerHpVar = CreateSO<IntVariableSO>();
            var onValueChanged = CreateSO<IntEventChannelSO>();
            SetField(playerHpVar, "onValueChanged", onValueChanged);
            SetField(playerHpVar, "initialValue", 5);

            playerHpVar.Value = 5;
            playerHpVar.Value = 2;

            int receivedValue = -1;
            onValueChanged.OnEventRaised += v => receivedValue = v;

            // Act
            playerHpVar.ResetToInitial();

            // Assert
            Assert.That(receivedValue, Is.EqualTo(5));
        }

        #endregion

        #region Entity Set

        [Test]
        public void EntitySetClear_ShouldResetCount()
        {
            // Arrange
            var enemySet = CreateSO<EnemyStateSetSO>();
            var defaultState = new EnemyState
            {
                Position = float2.zero,
                Velocity = float2.zero,
                Speed = 1f,
                Hp = 3,
                Polarity = 0,
                TypeId = EnemyTypeId.NWay
            };

            enemySet.Register(100, defaultState);
            enemySet.Register(101, defaultState);
            enemySet.Register(102, defaultState);
            Assert.That(enemySet.Count, Is.EqualTo(3));

            // Act
            enemySet.Clear();

            // Assert
            Assert.That(enemySet.Count, Is.EqualTo(0));
        }

        [Test]
        public void EntitySet_RegisterAndUnregister_ShouldTrackCount()
        {
            // Arrange
            var enemySet = CreateSO<EnemyStateSetSO>();
            var defaultState = new EnemyState { Hp = 3, Speed = 1f };

            // Act
            enemySet.Register(200, defaultState);
            enemySet.Register(201, defaultState);

            // Assert
            Assert.That(enemySet.Count, Is.EqualTo(2));

            // Act
            enemySet.Unregister(200);

            // Assert
            Assert.That(enemySet.Count, Is.EqualTo(1));
        }

        #endregion

        #region Event Chains

        [Test]
        public void EventChain_GameOver_ShouldStopRunAndSetResult()
        {
            // Arrange
            var onGameOver = CreateSO<VoidEventChannelSO>();
            var onRunStopped = CreateSO<VoidEventChannelSO>();

            bool gameOverFired = false;
            bool runStoppedFired = false;

            onGameOver.OnEventRaised += () =>
            {
                gameOverFired = true;
                // Simulate: controller stops run and fires onRunStopped
                onRunStopped.RaiseEvent();
            };
            onRunStopped.OnEventRaised += () => runStoppedFired = true;

            // Act
            onGameOver.RaiseEvent();

            // Assert
            Assert.That(gameOverFired, Is.True);
            Assert.That(runStoppedFired, Is.True);
        }

        [Test]
        public void EventChain_BossTrigger_ShouldChangePhase()
        {
            // Arrange
            var onBossTriggerReached = CreateSO<VoidEventChannelSO>();
            var gamePhaseVar = CreateSO<IntVariableSO>();
            gamePhaseVar.Value = (int)GamePhase.Stage;

            onBossTriggerReached.OnEventRaised += () =>
            {
                gamePhaseVar.Value = (int)GamePhase.Boss;
            };

            // Act
            onBossTriggerReached.RaiseEvent();

            // Assert
            Assert.That(gamePhaseVar.Value, Is.EqualTo((int)GamePhase.Boss));
        }

        [Test]
        public void EventChain_BossDefeated_ShouldTransitionToResult()
        {
            // Arrange
            var onBossDefeated = CreateSO<VoidEventChannelSO>();
            var resultTypeVar = CreateSO<IntVariableSO>();
            var gamePhaseVar = CreateSO<IntVariableSO>();
            gamePhaseVar.Value = (int)GamePhase.Boss;

            onBossDefeated.OnEventRaised += () =>
            {
                resultTypeVar.Value = (int)GameResultType.Clear;
                gamePhaseVar.Value = (int)GamePhase.Result;
            };

            // Act
            onBossDefeated.RaiseEvent();

            // Assert
            Assert.That(resultTypeVar.Value, Is.EqualTo((int)GameResultType.Clear));
            Assert.That(gamePhaseVar.Value, Is.EqualTo((int)GamePhase.Result));
        }

        [Test]
        public void EventChain_ResultRetry_ShouldTransitionToStage()
        {
            // Arrange
            var onResultRetrySelected = CreateSO<VoidEventChannelSO>();
            var gamePhaseVar = CreateSO<IntVariableSO>();
            gamePhaseVar.Value = (int)GamePhase.Result;

            onResultRetrySelected.OnEventRaised += () =>
            {
                gamePhaseVar.Value = (int)GamePhase.Stage;
            };

            // Act
            onResultRetrySelected.RaiseEvent();

            // Assert
            Assert.That(gamePhaseVar.Value, Is.EqualTo((int)GamePhase.Stage));
        }

        #endregion

        #region Score

        [Test]
        public void ScoreVar_AccumulatesAcrossEvents()
        {
            // Arrange
            var scoreVar = CreateSO<IntVariableSO>();
            var onScoreAdded = CreateSO<IntEventChannelSO>();

            onScoreAdded.OnEventRaised += points =>
            {
                scoreVar.Value = scoreVar.Value + points;
            };

            // Act
            onScoreAdded.RaiseEvent(100);
            onScoreAdded.RaiseEvent(250);
            onScoreAdded.RaiseEvent(50);

            // Assert
            Assert.That(scoreVar.Value, Is.EqualTo(400));
        }

        [Test]
        public void ScoreVar_ResetToInitial_ShouldClearScore()
        {
            // Arrange
            var scoreVar = CreateSO<IntVariableSO>();
            SetField(scoreVar, "initialValue", 0);
            scoreVar.Value = 500;

            // Act
            scoreVar.ResetToInitial();

            // Assert
            Assert.That(scoreVar.Value, Is.EqualTo(0));
        }

        #endregion

        #region Combo

        [Test]
        public void ComboReset_OnDamage_ShouldResetCombo()
        {
            // Arrange
            var comboCountVar = CreateSO<IntVariableSO>();
            var onPlayerDamaged = CreateSO<VoidEventChannelSO>();

            comboCountVar.Value = 5;

            onPlayerDamaged.OnEventRaised += () =>
            {
                comboCountVar.Value = 0;
            };

            // Act
            onPlayerDamaged.RaiseEvent();

            // Assert
            Assert.That(comboCountVar.Value, Is.EqualTo(0));
        }

        [Test]
        public void ComboIncrement_OnHit_ShouldAccumulate()
        {
            // Arrange
            var comboCountVar = CreateSO<IntVariableSO>();
            var onEnemyHit = CreateSO<VoidEventChannelSO>();

            onEnemyHit.OnEventRaised += () =>
            {
                comboCountVar.Value = comboCountVar.Value + 1;
            };

            // Act
            onEnemyHit.RaiseEvent();
            onEnemyHit.RaiseEvent();
            onEnemyHit.RaiseEvent();

            // Assert
            Assert.That(comboCountVar.Value, Is.EqualTo(3));
        }

        #endregion

        #region Spin Gauge

        [Test]
        public void SpinGauge_SetValue_ShouldStoreFloat()
        {
            // Arrange
            var spinGaugeVar = CreateSO<FloatVariableSO>();

            // Act
            spinGaugeVar.Value = 0.5f;

            // Assert
            Assert.That(spinGaugeVar.Value, Is.EqualTo(0.5f));
        }

        [Test]
        public void SpinGauge_ResetToInitial_ShouldRestoreZero()
        {
            // Arrange
            var spinGaugeVar = CreateSO<FloatVariableSO>();
            SetField(spinGaugeVar, "initialValue", 0f);
            spinGaugeVar.Value = 0.75f;

            // Act
            spinGaugeVar.ResetToInitial();

            // Assert
            Assert.That(spinGaugeVar.Value, Is.EqualTo(0f));
        }

        [Test]
        public void SpinGauge_ValueChanged_ShouldFireEvent()
        {
            // Arrange
            var spinGaugeVar = CreateSO<FloatVariableSO>();
            var onGaugeChanged = CreateSO<FloatEventChannelSO>();
            SetField(spinGaugeVar, "onValueChanged", onGaugeChanged);

            float receivedValue = -1f;
            onGaugeChanged.OnEventRaised += v => receivedValue = v;

            // Act
            spinGaugeVar.Value = 0.8f;

            // Assert
            Assert.That(receivedValue, Is.EqualTo(0.8f));
        }

        #endregion

        #region Multi-Variable Coordination

        [Test]
        public void PhaseTransition_ShouldResetPlayerHpAndScore()
        {
            // Arrange
            var playerHpVar = CreateSO<IntVariableSO>();
            var scoreVar = CreateSO<IntVariableSO>();
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);
            SetField(playerHpVar, "initialValue", 5);
            SetField(scoreVar, "initialValue", 0);

            playerHpVar.Value = 5;
            playerHpVar.Value = 2;
            scoreVar.Value = 300;

            // Simulate: phase change to Stage resets variables
            onGamePhaseChanged.OnEventRaised += phase =>
            {
                if (phase == (int)GamePhase.Stage)
                {
                    playerHpVar.ResetToInitial();
                    scoreVar.ResetToInitial();
                }
            };

            // Act
            gamePhaseVar.Value = (int)GamePhase.Stage;

            // Assert
            Assert.That(playerHpVar.Value, Is.EqualTo(5));
            Assert.That(scoreVar.Value, Is.EqualTo(0));
        }

        [Test]
        public void FullGameCycle_Title_Stage_Result_Title()
        {
            // Arrange
            var gamePhaseVar = CreateSO<IntVariableSO>();
            var onGamePhaseChanged = CreateSO<IntEventChannelSO>();
            SetField(gamePhaseVar, "onValueChanged", onGamePhaseChanged);

            var phaseHistory = new List<int>();
            onGamePhaseChanged.OnEventRaised += phase => phaseHistory.Add(phase);

            // Act - simulate full game cycle
            gamePhaseVar.Value = (int)GamePhase.Title;
            gamePhaseVar.Value = (int)GamePhase.Stage;
            gamePhaseVar.Value = (int)GamePhase.Boss;
            gamePhaseVar.Value = (int)GamePhase.Result;
            gamePhaseVar.Value = (int)GamePhase.Title;

            // Assert
            Assert.That(phaseHistory.Count, Is.EqualTo(5));
            Assert.That(phaseHistory[0], Is.EqualTo((int)GamePhase.Title));
            Assert.That(phaseHistory[1], Is.EqualTo((int)GamePhase.Stage));
            Assert.That(phaseHistory[2], Is.EqualTo((int)GamePhase.Boss));
            Assert.That(phaseHistory[3], Is.EqualTo((int)GamePhase.Result));
            Assert.That(phaseHistory[4], Is.EqualTo((int)GamePhase.Title));
        }

        #endregion

        // =====================================================================
        // GameFlowLogic Tests — Pure C# (Humble Object pattern)
        // =====================================================================
        // These tests instantiate GameFlowLogic directly with `new` and a
        // SpyGameFlowActions implementation of IGameFlowActions.
        // No MonoBehaviour, no Reflection, no Unity lifecycle.
        // =====================================================================

        #region GameFlowLogic: Test Infrastructure

        /// <summary>
        /// Spy implementation of IGameFlowActions that records all calls
        /// made by GameFlowLogic, enabling state and behavior verification.
        /// </summary>
        private class SpyGameFlowActions : IGameFlowActions
        {
            public int CloseTransitionCallCount { get; private set; }
            public int ConvergeTransitionToPlayerCallCount { get; private set; }
            public int ClearTransitionImmediateCallCount { get; private set; }

            public int LoadSceneCallCount { get; private set; }
            public List<string> LoadSceneHistory { get; } = new List<string>();

            public int SetGamePhaseVarCallCount { get; private set; }
            public int LastGamePhaseVarValue { get; private set; } = -1;

            public int SetResultTypeVarCallCount { get; private set; }
            public int LastResultTypeVarValue { get; private set; } = -1;

            public int RaiseGamePhaseChangedCallCount { get; private set; }
            public List<int> GamePhaseChangedHistory { get; } = new List<int>();

            public int RaiseBossPhaseRequestedCallCount { get; private set; }

            public int SaveTutorialCompletedCallCount { get; private set; }

            public float LastTransitionOriginX { get; private set; }
            public float LastTransitionOriginY { get; private set; }

            public void SetGamePhaseVariable(int phase)
            {
                SetGamePhaseVarCallCount++;
                LastGamePhaseVarValue = phase;
            }

            public void SetResultTypeVariable(int resultType)
            {
                SetResultTypeVarCallCount++;
                LastResultTypeVarValue = resultType;
            }

            public void CloseTransitionWithOrigin(float screenX, float screenY)
            {
                CloseTransitionCallCount++;
                LastTransitionOriginX = screenX;
                LastTransitionOriginY = screenY;
            }

            public void RaiseGamePhaseChanged(int phase)
            {
                RaiseGamePhaseChangedCallCount++;
                GamePhaseChangedHistory.Add(phase);
            }

            public void RaiseBossPhaseRequested()
            {
                RaiseBossPhaseRequestedCallCount++;
            }

            public void CloseTransition()
            {
                CloseTransitionCallCount++;
            }

            public void ConvergeTransitionToPlayer()
            {
                ConvergeTransitionToPlayerCallCount++;
            }

            public void ClearTransitionImmediate()
            {
                ClearTransitionImmediateCallCount++;
            }

            public void LoadScene(string sceneName)
            {
                LoadSceneCallCount++;
                LoadSceneHistory.Add(sceneName);
            }

            public void SaveTutorialCompleted()
            {
                SaveTutorialCompletedCallCount++;
            }
        }

        /// <summary>
        /// Creates a GameFlowLogic instance initialized to Title phase (simulating Start),
        /// paired with a fresh SpyGameFlowActions.
        /// </summary>
        private static (GameFlowLogic logic, SpyGameFlowActions spy) CreateInitializedLogic()
        {
            var spy = new SpyGameFlowActions();
            var logic = new GameFlowLogic(spy);
            logic.Initialize();
            return (logic, spy);
        }

        /// <summary>
        /// Advances logic from Title to Stage by simulating HandleTitleStartSelected
        /// followed by HandleTransitionClosed.
        /// </summary>
        private static void AdvanceToStage(GameFlowLogic logic)
        {
            logic.HandleTitleStartSelected();
            logic.HandleTransitionClosed();
        }

        /// <summary>
        /// Advances logic from Stage to Boss by simulating HandleBossTriggerReached.
        /// </summary>
        private static void AdvanceToBoss(GameFlowLogic logic)
        {
            logic.HandleBossTriggerReached();
        }

        /// <summary>
        /// Advances logic from current phase to Result via GameOver.
        /// </summary>
        private static void AdvanceToResultViaGameOver(GameFlowLogic logic)
        {
            logic.HandleGameOver();
            logic.HandleTransitionClosed();
        }

        /// <summary>
        /// Advances logic from current phase to Result via BossDefeated.
        /// </summary>
        private static void AdvanceToResultViaBossDefeated(GameFlowLogic logic)
        {
            logic.HandleBossDefeated();
            logic.HandleTransitionClosed();
        }

        #endregion

        // =====================================================================
        // 1. Initial State
        // =====================================================================

        #region GameFlowLogic: Initial State

        [Test]
        public void Controller_Start_ShouldSetPhaseToTitle()
        {
            // Arrange
            var spy = new SpyGameFlowActions();
            var logic = new GameFlowLogic(spy);

            // Act
            logic.Initialize();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title),
                "Initialize() should set currentPhase to Title.");
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Title),
                "Initialize() should write Title to gamePhaseVar.");
        }

        [Test]
        public void Controller_Start_ShouldFireGamePhaseChangedWithTitle()
        {
            // Arrange
            var spy = new SpyGameFlowActions();
            var logic = new GameFlowLogic(spy);

            // Act
            logic.Initialize();

            // Assert
            Assert.That(spy.RaiseGamePhaseChangedCallCount, Is.EqualTo(1));
            Assert.That(spy.GamePhaseChangedHistory[0], Is.EqualTo((int)GamePhase.Title),
                "Initialize() should raise onGamePhaseChanged with Title.");
        }

        [Test]
        public void Controller_Start_ShouldRequestTitleSceneLoad()
        {
            // Arrange
            var spy = new SpyGameFlowActions();
            var logic = new GameFlowLogic(spy);

            // Act
            logic.Initialize();

            // Assert
            Assert.That(spy.LoadSceneCallCount, Is.EqualTo(1));
            Assert.That(spy.LoadSceneHistory[0], Is.EqualTo("Title"),
                "Initialize() should request loading the Title scene.");
        }

        #endregion

        // =====================================================================
        // 2. Title -> Gameplay (Start pressed)
        // =====================================================================

        #region GameFlowLogic: Title -> Stage

        [Test]
        public void Controller_TitleStartSelected_ShouldSetPendingPhaseToStage()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();

            // Act
            logic.HandleTitleStartSelected();

            // Assert
            Assert.That(logic.PendingPhase, Is.EqualTo(GamePhase.Stage),
                "HandleTitleStartSelected should set pendingPhase to Stage.");
        }

        [Test]
        public void Controller_TitleStartSelected_WithOrigin_ShouldCallCloseTransitionWithOrigin()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Act
            logic.HandleTitleStartTransitionOriginSelected(123.5f, 456.0f);
            logic.HandleTitleStartSelected();

            // Assert
            Assert.That(spy.LastTransitionOriginX, Is.EqualTo(123.5f));
            Assert.That(spy.LastTransitionOriginY, Is.EqualTo(456.0f));
            Assert.That(spy.CloseTransitionCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Controller_TitleStartSelected_ShouldNotChangeCurrentPhase()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();

            // Act
            logic.HandleTitleStartSelected();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title),
                "Phase should NOT change until transition closes.");
        }

        [Test]
        public void Controller_TitleToStage_AfterTransitionClosed_ShouldChangePhaseToStage()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Act
            logic.HandleTitleStartSelected();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Stage),
                "After transition closes, phase should become Stage.");
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Stage),
                "gamePhaseVar should reflect Stage after transition closes.");
        }

        [Test]
        public void Controller_TitleToStage_ShouldFireGamePhaseChangedWithStage()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            // Clear history from Initialize
            spy.GamePhaseChangedHistory.Clear();

            // Act
            logic.HandleTitleStartSelected();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(spy.GamePhaseChangedHistory.Count, Is.EqualTo(1));
            Assert.That(spy.GamePhaseChangedHistory[0], Is.EqualTo((int)GamePhase.Stage));
        }

        [Test]
        public void Controller_TitleStartSelected_ShouldCallCloseTransition()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            int closeCountBefore = spy.CloseTransitionCallCount;

            // Act
            logic.HandleTitleStartSelected();

            // Assert
            Assert.That(spy.CloseTransitionCallCount, Is.EqualTo(closeCountBefore + 1),
                "HandleTitleStartSelected should call CloseTransition on actions.");
        }

        #endregion

        // =====================================================================
        // 3. Gameplay -> Result (GameOver)
        // =====================================================================

        #region GameFlowLogic: GameOver Flow

        [Test]
        public void Controller_GameOver_ShouldSetPendingToResultGameOver()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Act
            logic.HandleGameOver();

            // Assert
            Assert.That(logic.PendingPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(logic.PendingResultType, Is.EqualTo(GameResultType.GameOver));
        }

        [Test]
        public void Controller_GameOver_ShouldNotChangeCurrentPhaseUntilTransitionCloses()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Act
            logic.HandleGameOver();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Stage),
                "Phase should remain Stage until transition closes.");
        }

        [Test]
        public void Controller_GameOver_AfterTransitionClosed_ShouldSetResultTypeVarAndPhase()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Act
            logic.HandleGameOver();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Result));
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.GameOver),
                "resultTypeVar should be set to GameOver when transitioning to Result.");
        }

        [Test]
        public void Controller_GameOver_ShouldCallConvergeTransitionToPlayer()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Act
            logic.HandleGameOver();

            // Assert
            Assert.That(spy.ConvergeTransitionToPlayerCallCount, Is.EqualTo(1),
                "HandleGameOver should call ConvergeTransitionToPlayer (not CloseTransition).");
        }

        #endregion

        // =====================================================================
        // 4. Gameplay -> Boss -> Result (Clear)
        // =====================================================================

        #region GameFlowLogic: Boss Flow

        [Test]
        public void Controller_BossTriggerReached_ShouldImmediatelyTransitionToBoss()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Act
            logic.HandleBossTriggerReached();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Boss),
                "HandleBossTriggerReached should call TransitionTo(Boss) immediately.");
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Boss));
        }

        [Test]
        public void Controller_BossTriggerReached_ShouldRaiseBossPhaseRequested()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Act
            logic.HandleBossTriggerReached();

            // Assert
            Assert.That(spy.RaiseBossPhaseRequestedCallCount, Is.GreaterThan(0),
                "HandleBossTriggerReached should raise onBossPhaseRequested.");
        }

        [Test]
        public void Controller_BossDefeated_ShouldSetPendingResultClear()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToBoss(logic);

            // Act
            logic.HandleBossDefeated();

            // Assert
            Assert.That(logic.PendingPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(logic.PendingResultType, Is.EqualTo(GameResultType.Clear));
        }

        [Test]
        public void Controller_BossDefeated_AfterTransitionClosed_ShouldSetResultTypeClear()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToBoss(logic);

            // Act
            logic.HandleBossDefeated();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.Clear),
                "resultTypeVar should be Clear after boss defeated.");
        }

        #endregion

        // =====================================================================
        // 5. Result -> Retry (back to Gameplay)
        // =====================================================================

        #region GameFlowLogic: Result -> Retry

        [Test]
        public void Controller_ResultRetrySelected_ShouldSetPendingPhaseToStage()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToResultViaGameOver(logic);

            // Act
            logic.HandleResultRetrySelected();

            // Assert
            Assert.That(logic.PendingPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Result),
                "Phase should remain Result until transition closes.");
        }

        [Test]
        public void Controller_ResultRetry_AfterTransitionClosed_ShouldChangeToStage()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToResultViaGameOver(logic);

            // Act
            logic.HandleResultRetrySelected();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Stage));
        }

        #endregion

        // =====================================================================
        // 6. Result -> Title
        // =====================================================================

        #region GameFlowLogic: Result -> Title

        [Test]
        public void Controller_ResultBackToTitleSelected_ShouldSetPendingPhaseToTitle()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToResultViaGameOver(logic);

            // Act
            logic.HandleResultBackToTitleSelected();

            // Assert
            Assert.That(logic.PendingPhase, Is.EqualTo(GamePhase.Title));
        }

        [Test]
        public void Controller_ResultBackToTitle_AfterTransitionClosed_ShouldChangeToTitle()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToResultViaGameOver(logic);

            // Act
            logic.HandleResultBackToTitleSelected();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title));
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Title));
        }

        #endregion

        // =====================================================================
        // 7. HandleSceneLoadCompleted
        // =====================================================================

        #region GameFlowLogic: HandleSceneLoadCompleted

        [Test]
        public void Controller_HandleSceneLoadCompleted_ShouldCallClearTransitionImmediateWithoutChangingPhase()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            var phaseBefore = logic.CurrentPhase;

            // Act
            logic.HandleSceneLoadCompleted();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(phaseBefore),
                "HandleSceneLoadCompleted should not change the current phase.");
            Assert.That(spy.ClearTransitionImmediateCallCount, Is.GreaterThan(0),
                "HandleSceneLoadCompleted should call ClearTransitionImmediate.");
        }

        #endregion

        // =====================================================================
        // 8. HandleTutorialCompleted
        // =====================================================================

        #region GameFlowLogic: Tutorial Completed

        [Test]
        public void Controller_TutorialCompleted_ShouldSetPendingTitleAndCallSave()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            var phaseBefore = logic.CurrentPhase;

            // Act
            logic.HandleTutorialCompleted();

            // Assert
            Assert.That(logic.PendingPhase, Is.EqualTo(GamePhase.Title));
            Assert.That(logic.CurrentPhase, Is.EqualTo(phaseBefore),
                "Current phase should not change until the transition closes.");
            Assert.That(spy.SaveTutorialCompletedCallCount, Is.EqualTo(1));
            Assert.That(spy.CloseTransitionCallCount, Is.GreaterThan(0));
        }

        [Test]
        public void Controller_TutorialCompleted_AfterTransitionClosed_ShouldChangeToTitle()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Act
            logic.HandleTutorialCompleted();
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title));
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Title));
        }

        #endregion

        // =====================================================================
        // 9. Full Lifecycle Scenarios
        // =====================================================================

        #region GameFlowLogic: Full Lifecycle

        [Test]
        public void Controller_FullLifecycle_TitleStageBossResultTitle()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Title (from Initialize)
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title));

            // Title -> Stage
            AdvanceToStage(logic);
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Stage));

            // Stage -> Boss (immediate)
            logic.HandleBossTriggerReached();
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Boss));

            // Boss -> Result (Clear)
            AdvanceToResultViaBossDefeated(logic);
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.Clear));

            // Result -> Title
            logic.HandleResultBackToTitleSelected();
            logic.HandleTransitionClosed();
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title));

            // Verify phase history: Title, Stage, Boss, Result, Title
            Assert.That(spy.GamePhaseChangedHistory, Is.EqualTo(new[]
            {
                (int)GamePhase.Title,
                (int)GamePhase.Stage,
                (int)GamePhase.Boss,
                (int)GamePhase.Result,
                (int)GamePhase.Title,
            }));
        }

        [Test]
        public void Controller_FullLifecycle_TitleStageGameOverRetryGameOverTitle()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Title -> Stage
            AdvanceToStage(logic);

            // Stage -> Result (GameOver)
            AdvanceToResultViaGameOver(logic);
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.GameOver));

            // Result -> Retry (Stage)
            logic.HandleResultRetrySelected();
            logic.HandleTransitionClosed();

            // Stage -> Result (GameOver again)
            AdvanceToResultViaGameOver(logic);
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.GameOver));

            // Result -> Title
            logic.HandleResultBackToTitleSelected();
            logic.HandleTransitionClosed();

            // Verify phase history: Title, Stage, Result, Stage, Result, Title
            Assert.That(spy.GamePhaseChangedHistory, Is.EqualTo(new[]
            {
                (int)GamePhase.Title,
                (int)GamePhase.Stage,
                (int)GamePhase.Result,
                (int)GamePhase.Stage,
                (int)GamePhase.Result,
                (int)GamePhase.Title,
            }));
        }

        #endregion

        // =====================================================================
        // 10. Variable/Event Consistency
        // =====================================================================

        #region GameFlowLogic: Variable/Event Consistency

        [Test]
        public void Controller_TransitionTo_ShouldSyncGamePhaseVarAndEvent()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Act - trigger Title -> Stage
            AdvanceToStage(logic);

            // Assert
            Assert.That(spy.LastGamePhaseVarValue, Is.EqualTo((int)GamePhase.Stage));
            Assert.That(spy.GamePhaseChangedHistory[spy.GamePhaseChangedHistory.Count - 1],
                Is.EqualTo((int)GamePhase.Stage),
                "onGamePhaseChanged event value should match gamePhaseVar.");
        }

        [Test]
        public void Controller_ResultTypeVar_ShouldOnlyBeWrittenForResultTransitions()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Act - Title -> Stage (should NOT write resultTypeVar)
            AdvanceToStage(logic);
            int resultWriteCountAfterStage = spy.SetResultTypeVarCallCount;

            // Assert
            Assert.That(resultWriteCountAfterStage, Is.EqualTo(0),
                "resultTypeVar should not be written during Title -> Stage transition.");

            // Act - Stage -> Result (GameOver) - SHOULD write resultTypeVar
            AdvanceToResultViaGameOver(logic);

            // Assert
            Assert.That(spy.SetResultTypeVarCallCount, Is.EqualTo(1),
                "resultTypeVar should be written once when transitioning to Result.");
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.GameOver));
        }

        [Test]
        public void Controller_ResultTypeVar_ShouldUpdateFromGameOverToClear()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();

            // Title -> Stage -> GameOver -> Result
            AdvanceToStage(logic);
            AdvanceToResultViaGameOver(logic);
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.GameOver));

            // Result -> Retry -> Stage -> Boss -> BossDefeated -> Result
            logic.HandleResultRetrySelected();
            logic.HandleTransitionClosed();
            logic.HandleBossTriggerReached();
            AdvanceToResultViaBossDefeated(logic);

            // Assert
            Assert.That(spy.LastResultTypeVarValue, Is.EqualTo((int)GameResultType.Clear),
                "After boss defeated, resultTypeVar should be Clear.");
        }

        #endregion

        // =====================================================================
        // 11. GameOver should NOT fire BossPhaseRequested
        // =====================================================================

        #region GameFlowLogic: Negative Cases

        [Test]
        public void Controller_GameOver_ShouldNotFireBossPhaseRequested()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);
            int bossRequestedBefore = spy.RaiseBossPhaseRequestedCallCount;

            // Act
            AdvanceToResultViaGameOver(logic);

            // Assert
            Assert.That(spy.RaiseBossPhaseRequestedCallCount, Is.EqualTo(bossRequestedBefore),
                "GameOver flow should not raise onBossPhaseRequested.");
        }

        #endregion

        // =====================================================================
        // 12. Deferred vs Immediate Transitions
        // =====================================================================

        #region GameFlowLogic: Deferred vs Immediate

        [Test]
        public void Controller_DeferredTransitions_AllRequireTransitionClose()
        {
            // Arrange
            var (logic, _) = CreateInitializedLogic();

            // Title -> attempt Stage (without closing transition)
            logic.HandleTitleStartSelected();
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Title),
                "Title start should not change phase without transition close.");

            // Close to complete
            logic.HandleTransitionClosed();
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Stage));

            // Stage -> attempt Result via GameOver (without closing)
            logic.HandleGameOver();
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Stage),
                "GameOver should not change phase without transition close.");

            // Close to complete
            logic.HandleTransitionClosed();
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Result));
        }

        [Test]
        public void Controller_BossTrigger_IsImmediateTransition()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Clear history to count only from this point
            spy.GamePhaseChangedHistory.Clear();

            // Act
            logic.HandleBossTriggerReached();

            // Assert
            Assert.That(logic.CurrentPhase, Is.EqualTo(GamePhase.Boss));
            Assert.That(spy.GamePhaseChangedHistory.Count, Is.EqualTo(1),
                "Boss trigger should fire exactly one phase change event immediately.");
        }

        #endregion

        // =====================================================================
        // 13. Event Fire Counts
        // =====================================================================

        #region GameFlowLogic: Event Fire Counts

        [Test]
        public void Controller_BossFlow_ShouldFirePhaseChangedTwice_BossAndResult()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);

            // Clear history to count only from this point
            spy.GamePhaseChangedHistory.Clear();

            // Act
            logic.HandleBossTriggerReached();
            AdvanceToResultViaBossDefeated(logic);

            // Assert
            Assert.That(spy.GamePhaseChangedHistory.Count, Is.EqualTo(2));
            Assert.That(spy.GamePhaseChangedHistory[0], Is.EqualTo((int)GamePhase.Boss));
            Assert.That(spy.GamePhaseChangedHistory[1], Is.EqualTo((int)GamePhase.Result));
        }

        [Test]
        public void Controller_FullCycle_PhaseChangeCount()
        {
            // Arrange
            var spy = new SpyGameFlowActions();
            var logic = new GameFlowLogic(spy);

            logic.Initialize(); // Title = 1

            AdvanceToStage(logic); // Stage = 2

            AdvanceToResultViaGameOver(logic); // Result = 3

            logic.HandleResultBackToTitleSelected();
            logic.HandleTransitionClosed(); // Title = 4

            Assert.That(spy.RaiseGamePhaseChangedCallCount, Is.EqualTo(4),
                "Expected 4 phase changes: Title, Stage, Result, Title.");
        }

        #endregion

        // =====================================================================
        // 14. TransitionClosed Scene Loading
        // =====================================================================

        #region GameFlowLogic: TransitionClosed Scene Loading

        [Test]
        public void Controller_TransitionClosed_ForStage_ShouldLoadGameplayScene()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            logic.HandleTitleStartSelected();

            // Act
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(spy.LoadSceneHistory, Does.Contain("Gameplay"));
        }

        [Test]
        public void Controller_TransitionClosed_ForTitle_ShouldLoadTitleScene()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);
            AdvanceToResultViaGameOver(logic);
            logic.HandleResultBackToTitleSelected();

            // Act
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(spy.LoadSceneHistory[spy.LoadSceneHistory.Count - 1], Is.EqualTo("Title"));
        }

        [Test]
        public void Controller_TransitionClosed_ForResult_ShouldLoadResultScene()
        {
            // Arrange
            var (logic, spy) = CreateInitializedLogic();
            AdvanceToStage(logic);
            logic.HandleGameOver();

            // Act
            logic.HandleTransitionClosed();

            // Assert
            Assert.That(spy.LoadSceneHistory[spy.LoadSceneHistory.Count - 1], Is.EqualTo("Result"));
        }

        #endregion

        // =====================================================================
        // Existing simulation-based tests (preserved)
        // =====================================================================

        #region Helpers — Simulated Flow Controller State

        /// <summary>
        /// Lightweight struct mirroring the mutable state of GameFlowController
        /// so we can test the event-chain logic in pure C# without MonoBehaviours.
        /// </summary>
        private class SimulatedFlowState
        {
            public GamePhase CurrentPhase;
            public GamePhase PendingPhase;
            public GameResultType PendingResultType;
            public bool IsInitialLoad = true;

            public IntVariableSO GamePhaseVar;
            public IntVariableSO ResultTypeVar;
            public IntEventChannelSO OnGamePhaseChanged;
            public VoidEventChannelSO OnBossPhaseRequested;

            /// <summary>Mirrors GameFlowController.TransitionTo.</summary>
            public void TransitionTo(GamePhase next)
            {
                CurrentPhase = next;
                if (GamePhaseVar != null)
                    GamePhaseVar.Value = (int)CurrentPhase;
                OnGamePhaseChanged?.RaiseEvent((int)CurrentPhase);
            }
        }

        /// <summary>
        /// Creates a fully wired SimulatedFlowState with all events and variables,
        /// mimicking what GameFlowController.OnEnable does.
        /// </summary>
        private SimulatedFlowState CreateFlowState(
            VoidEventChannelSO onGameOver,
            VoidEventChannelSO onTitleStartSelected,
            VoidEventChannelSO onBossTriggerReached,
            VoidEventChannelSO onBossDefeated,
            VoidEventChannelSO onResultRetrySelected,
            VoidEventChannelSO onResultBackToTitleSelected,
            VoidEventChannelSO onScreenTransitionClosed,
            VoidEventChannelSO onSceneLoadCompleted,
            VoidEventChannelSO onScreenTransitionOpened,
            VoidEventChannelSO onTutorialCompleted = null)
        {
            var state = new SimulatedFlowState
            {
                GamePhaseVar = CreateSO<IntVariableSO>(),
                ResultTypeVar = CreateSO<IntVariableSO>(),
                OnGamePhaseChanged = CreateSO<IntEventChannelSO>(),
                OnBossPhaseRequested = CreateSO<VoidEventChannelSO>(),
            };

            // --- Wire event handlers (mirrors GameFlowController.OnEnable) ---

            // onGameOver -> set pending Result/GameOver, close transition
            onGameOver.OnEventRaised += () =>
            {
                state.PendingPhase = GamePhase.Result;
                state.PendingResultType = GameResultType.GameOver;
                // In real controller: screenTransitionController.Close()
                // which eventually fires onScreenTransitionClosed
            };

            // onTitleStartSelected -> set pending Stage, close transition
            onTitleStartSelected.OnEventRaised += () =>
            {
                state.PendingPhase = GamePhase.Stage;
            };

            // onBossTriggerReached -> fire boss phase requested, immediate transition
            onBossTriggerReached.OnEventRaised += () =>
            {
                state.OnBossPhaseRequested.RaiseEvent();
                state.TransitionTo(GamePhase.Boss);
            };

            // onBossDefeated -> set pending Result/Clear
            onBossDefeated.OnEventRaised += () =>
            {
                state.PendingPhase = GamePhase.Result;
                state.PendingResultType = GameResultType.Clear;
            };

            // onResultRetrySelected -> set pending Stage
            onResultRetrySelected.OnEventRaised += () =>
            {
                state.PendingPhase = GamePhase.Stage;
            };

            // onResultBackToTitleSelected -> set pending Title
            onResultBackToTitleSelected.OnEventRaised += () =>
            {
                state.PendingPhase = GamePhase.Title;
            };

            // onScreenTransitionClosed -> TransitionTo(pending), set resultTypeVar, trigger scene load
            onScreenTransitionClosed.OnEventRaised += () =>
            {
                state.TransitionTo(state.PendingPhase);

                if (state.PendingPhase == GamePhase.Result && state.ResultTypeVar != null)
                {
                    state.ResultTypeVar.Value = (int)state.PendingResultType;
                }
            };

            // onTutorialCompleted -> set pending Title, close transition
            if (onTutorialCompleted != null)
            {
                onTutorialCompleted.OnEventRaised += () =>
                {
                    state.PendingPhase = GamePhase.Title;
                    // In real controller: screenTransitionController.Close()
                };
            }

            // onSceneLoadCompleted -> skip Open on initial load, then Open on subsequent loads
            onSceneLoadCompleted.OnEventRaised += () =>
            {
                if (state.IsInitialLoad)
                {
                    state.IsInitialLoad = false;
                    return;
                }
                // In real controller: screenTransitionController.Open()
                // which eventually fires onScreenTransitionOpened
                onScreenTransitionOpened?.RaiseEvent();
            };

            // onScreenTransitionOpened -> no-op in controller

            return state;
        }

        /// <summary>
        /// Convenience: creates all event channels needed by the flow controller.
        /// </summary>
        private (
            VoidEventChannelSO onGameOver,
            VoidEventChannelSO onTitleStartSelected,
            VoidEventChannelSO onBossTriggerReached,
            VoidEventChannelSO onBossDefeated,
            VoidEventChannelSO onResultRetrySelected,
            VoidEventChannelSO onResultBackToTitleSelected,
            VoidEventChannelSO onScreenTransitionClosed,
            VoidEventChannelSO onSceneLoadCompleted,
            VoidEventChannelSO onScreenTransitionOpened,
            VoidEventChannelSO onTutorialCompleted
        ) CreateAllChannels()
        {
            return (
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>(),
                CreateSO<VoidEventChannelSO>()
            );
        }

        #endregion

        #region Scenario: Title -> Stage Flow

        [Test]
        public void TitleStartSelected_ShouldSetPendingPhaseToStage()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Title);

            // Act
            ch.onTitleStartSelected.RaiseEvent();

            // Assert
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Stage),
                "onTitleStartSelected should set pendingPhase to Stage.");
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Title),
                "Phase should NOT change until transition closes.");
        }

        [Test]
        public void TitleToStage_AfterTransitionClose_ShouldChangePhaseToStage()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Title);

            // Act - title start -> transition closes
            ch.onTitleStartSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Stage));
        }

        [Test]
        public void TitleToStage_GamePhaseChangedEvent_ShouldFireWithStage()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Title);

            int receivedPhase = -1;
            int fireCount = 0;
            state.OnGamePhaseChanged.OnEventRaised += p =>
            {
                receivedPhase = p;
                fireCount++;
            };

            // Act
            ch.onTitleStartSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert - onGamePhaseChanged should have fired exactly once (for Stage)
            Assert.That(fireCount, Is.EqualTo(1));
            Assert.That(receivedPhase, Is.EqualTo((int)GamePhase.Stage));
        }

        #endregion

        #region Scenario: GameOver Flow

        [Test]
        public void GameOver_ShouldSetPendingPhaseToResult_AndResultTypeToGameOver()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            // Act
            ch.onGameOver.RaiseEvent();

            // Assert
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.PendingResultType, Is.EqualTo(GameResultType.GameOver));
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage),
                "Phase should NOT change until transition closes.");
        }

        [Test]
        public void GameOver_AfterTransitionClose_ShouldSetResultTypeVarAndChangePhase()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            // Act
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Result));
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.GameOver));
        }

        [Test]
        public void GameOver_ShouldNotFireBossPhaseRequested()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            bool bossPhaseRequested = false;
            state.OnBossPhaseRequested.OnEventRaised += () => bossPhaseRequested = true;

            // Act
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(bossPhaseRequested, Is.False);
        }

        #endregion

        #region Scenario: Boss Flow

        [Test]
        public void BossTriggerReached_ShouldFireBossPhaseRequested()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            bool bossPhaseRequested = false;
            state.OnBossPhaseRequested.OnEventRaised += () => bossPhaseRequested = true;

            // Act
            ch.onBossTriggerReached.RaiseEvent();

            // Assert
            Assert.That(bossPhaseRequested, Is.True);
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Boss));
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Boss));
        }

        [Test]
        public void BossTriggerReached_ShouldChangePhaseImmediately_WithoutTransition()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            // Act - boss trigger fires
            ch.onBossTriggerReached.RaiseEvent();

            // Assert - phase changes immediately (no pendingPhase / transition close needed)
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Boss),
                "Boss phase should be set immediately, not deferred to transition close.");
        }

        [Test]
        public void BossDefeated_ShouldSetPendingResultClear()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Boss);

            // Act
            ch.onBossDefeated.RaiseEvent();

            // Assert
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.PendingResultType, Is.EqualTo(GameResultType.Clear));
        }

        [Test]
        public void BossDefeated_AfterTransitionClose_ShouldSetResultTypeClear()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Boss);

            // Act
            ch.onBossDefeated.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.Clear));
        }

        #endregion

        #region Scenario: Result -> Retry Flow

        [Test]
        public void ResultRetrySelected_ShouldSetPendingPhaseToStage()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Result);

            // Act
            ch.onResultRetrySelected.RaiseEvent();

            // Assert
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result),
                "Phase should NOT change until transition closes.");
        }

        [Test]
        public void ResultRetry_AfterTransitionClose_ShouldChangePhaseToStage()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Result);

            // Act
            ch.onResultRetrySelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Stage));
        }

        #endregion

        #region Scenario: Result -> Title Flow

        [Test]
        public void ResultBackToTitleSelected_ShouldSetPendingPhaseToTitle()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Result);

            // Act
            ch.onResultBackToTitleSelected.RaiseEvent();

            // Assert
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Title));
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
        }

        [Test]
        public void ResultBackToTitle_AfterTransitionClose_ShouldChangePhaseToTitle()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Result);

            // Act
            ch.onResultBackToTitleSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Title));
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Title));
        }

        #endregion

        #region Scenario: Full Lifecycle

        [Test]
        public void FullLifecycle_Title_Stage_GameOver_Result_Retry_Stage_BossDefeated_Result_Title()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            var phaseHistory = new List<int>();
            state.OnGamePhaseChanged.OnEventRaised += p => phaseHistory.Add(p);

            // Act - Step 1: Start at Title
            state.TransitionTo(GamePhase.Title);
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Title));

            // Step 2: Title -> Stage
            ch.onTitleStartSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));

            // Step 3: Stage -> GameOver -> Result
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.GameOver));

            // Step 4: Result -> Retry -> Stage
            ch.onResultRetrySelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));

            // Step 5: Stage -> Boss
            ch.onBossTriggerReached.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Boss));

            // Step 6: Boss -> BossDefeated -> Result
            ch.onBossDefeated.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.Clear));

            // Step 7: Result -> Title
            ch.onResultBackToTitleSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Title));

            // Assert - full phase history
            Assert.That(phaseHistory, Is.EqualTo(new[]
            {
                (int)GamePhase.Title,
                (int)GamePhase.Stage,
                (int)GamePhase.Result,
                (int)GamePhase.Stage,
                (int)GamePhase.Boss,
                (int)GamePhase.Result,
                (int)GamePhase.Title,
            }));
        }

        [Test]
        public void FullLifecycle_PhaseChangeCount_ShouldMatchExpectedTransitions()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            int phaseChangeCount = 0;
            state.OnGamePhaseChanged.OnEventRaised += _ => phaseChangeCount++;

            // Act - Title -> Stage -> GameOver -> Result -> Title (5 transitions)
            state.TransitionTo(GamePhase.Title);
            ch.onTitleStartSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            ch.onResultBackToTitleSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(phaseChangeCount, Is.EqualTo(4),
                "Expected 4 phase changes: Title, Stage, Result, Title.");
        }

        #endregion

        #region Scenario: SceneTransitionTracker Integration

        [Test]
        public void SceneTransitionTracker_TryBeginTransition_BlocksDuringActiveTransition()
        {
            // Arrange
            var tracker = new SceneTransitionTracker();

            // Act - begin first transition
            bool first = tracker.TryBeginTransition("Gameplay");

            // Assert - second attempt should be blocked
            bool second = tracker.TryBeginTransition("Result");
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(tracker.IsTransitioning, Is.True);
        }

        [Test]
        public void SceneTransitionTracker_FullUnloadLoadCycle_TracksState()
        {
            // Arrange
            var tracker = new SceneTransitionTracker();
            tracker.SetLoadedScene("Title");

            // Act - begin transition
            Assert.That(tracker.TryBeginTransition("Gameplay"), Is.True);

            // Unload current scene
            tracker.BeginUnloadCurrent();
            Assert.That(tracker.LoadedContentSceneName, Is.Null);
            Assert.That(tracker.ShouldHandleUnloadEvent("Title"), Is.True);

            // Complete unload
            string unloaded = tracker.HandleUnloadCompleted();
            Assert.That(unloaded, Is.EqualTo("Title"));

            // Load new scene
            tracker.HandleLoadCompleted("Gameplay");
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));

            // End transition
            tracker.EndTransition();
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
        }

        [Test]
        public void SceneTransitionTracker_AfterEndTransition_AllowsNewTransition()
        {
            // Arrange
            var tracker = new SceneTransitionTracker();
            tracker.TryBeginTransition("Gameplay");
            tracker.EndTransition();

            // Act
            bool result = tracker.TryBeginTransition("Result");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SceneTransitionTracker_MultipleFullCycles_TrackCorrectly()
        {
            // Arrange
            var tracker = new SceneTransitionTracker();

            // Cycle 1: Title -> Gameplay
            Assert.That(tracker.TryBeginTransition("Gameplay"), Is.True);
            tracker.HandleLoadCompleted("Gameplay");
            tracker.EndTransition();
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));

            // Cycle 2: Gameplay -> Result (with unload)
            Assert.That(tracker.TryBeginTransition("Result"), Is.True);
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Result");
            tracker.EndTransition();
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));

            // Cycle 3: Result -> Title (with unload)
            Assert.That(tracker.TryBeginTransition("Title"), Is.True);
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Title");
            tracker.EndTransition();
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Title"));
        }

        #endregion

        #region Scenario: Event Fire Count Verification

        [Test]
        public void GameOverFlow_OnGamePhaseChanged_ShouldFireExactlyOnce()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            int fireCount = 0;
            state.OnGamePhaseChanged.OnEventRaised += _ => fireCount++;

            // Act
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(fireCount, Is.EqualTo(1),
                "onGamePhaseChanged should fire exactly once for the Result transition.");
        }

        [Test]
        public void BossFlow_OnGamePhaseChanged_ShouldFireOnceForBossAndOnceForResult()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            var firedPhases = new List<int>();
            state.OnGamePhaseChanged.OnEventRaised += p => firedPhases.Add(p);

            // Act - boss trigger (immediate) -> boss defeated -> transition close (deferred)
            ch.onBossTriggerReached.RaiseEvent();
            ch.onBossDefeated.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(firedPhases.Count, Is.EqualTo(2));
            Assert.That(firedPhases[0], Is.EqualTo((int)GamePhase.Boss));
            Assert.That(firedPhases[1], Is.EqualTo((int)GamePhase.Result));
        }

        [Test]
        public void MultipleTransitions_EventFireCounts_ShouldBeAccurate()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            int totalPhaseChanges = 0;
            state.OnGamePhaseChanged.OnEventRaised += _ => totalPhaseChanges++;

            // Act - Title -> Stage -> Result(GameOver) = 3 transitions
            state.TransitionTo(GamePhase.Title);
            ch.onTitleStartSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(totalPhaseChanges, Is.EqualTo(3),
                "Expected 3 phase changes: Title(1), Stage(2), Result(3).");
        }

        #endregion

        #region Scenario: ResultType Preservation

        [Test]
        public void GameOverThenRetryThenBossDefeated_ResultTypeShouldBeClear()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            // Act - game over
            ch.onGameOver.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.GameOver));

            // Retry
            ch.onResultRetrySelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Boss trigger -> boss defeated
            ch.onBossTriggerReached.RaiseEvent();
            ch.onBossDefeated.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert - result type should now be Clear, not GameOver
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.Clear),
                "After boss defeated, resultType should be Clear regardless of previous GameOver.");
        }

        [Test]
        public void ResultTypeVar_ShouldOnlyBeSet_WhenPendingPhaseIsResult()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Title);

            var onResultTypeChanged = CreateSO<IntEventChannelSO>();
            SetField(state.ResultTypeVar, "onValueChanged", onResultTypeChanged);

            int resultTypeFireCount = 0;
            onResultTypeChanged.OnEventRaised += _ => resultTypeFireCount++;

            // Act - Title -> Stage (resultTypeVar should NOT be written)
            ch.onTitleStartSelected.RaiseEvent();
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert
            Assert.That(resultTypeFireCount, Is.EqualTo(0),
                "resultTypeVar should not be written during Title -> Stage transition.");
        }

        #endregion

        #region Scenario: Deferred vs Immediate Transitions

        [Test]
        public void DeferredTransitions_ShouldNotChangePhaseUntilTransitionCloses()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            // Act - fire game over but do NOT fire transition close
            ch.onGameOver.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage),
                "Deferred transition should not change currentPhase until onScreenTransitionClosed fires.");
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Result));
        }

        [Test]
        public void ImmediateTransition_BossTrigger_ChangesPhaseWithoutClose()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            state.TransitionTo(GamePhase.Stage);

            int phaseChanges = 0;
            state.OnGamePhaseChanged.OnEventRaised += _ => phaseChanges++;

            // Act - fire boss trigger (no transition close needed)
            ch.onBossTriggerReached.RaiseEvent();

            // Assert
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Boss));
            Assert.That(phaseChanges, Is.EqualTo(1),
                "Boss trigger is an immediate transition; phase should change without waiting.");
        }

        #endregion

        #region Scenario: SceneTransitionTracker + Event Chains Combined

        [Test]
        public void Combined_TitleToStage_TrackerAndEventsCoordinate()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            var tracker = new SceneTransitionTracker();
            tracker.SetLoadedScene("Title");

            state.TransitionTo(GamePhase.Title);

            // Act - title start selected
            ch.onTitleStartSelected.RaiseEvent();

            // Begin tracker transition
            Assert.That(tracker.TryBeginTransition("Gameplay"), Is.True);

            // Transition closes -> phase changes
            ch.onScreenTransitionClosed.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));

            // Unload Title
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();

            // Load Gameplay
            tracker.HandleLoadCompleted("Gameplay");
            ch.onSceneLoadCompleted.RaiseEvent();

            // End tracker transition
            tracker.EndTransition();

            // Assert
            Assert.That(tracker.IsTransitioning, Is.False);
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Stage));
        }

        [Test]
        public void Combined_FullCycle_TrackerAndEvents_StayInSync()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened);

            var tracker = new SceneTransitionTracker();

            state.TransitionTo(GamePhase.Title);

            // --- Title -> Gameplay ---
            ch.onTitleStartSelected.RaiseEvent();
            Assert.That(tracker.TryBeginTransition("Gameplay"), Is.True);
            ch.onScreenTransitionClosed.RaiseEvent();
            tracker.HandleLoadCompleted("Gameplay");
            ch.onSceneLoadCompleted.RaiseEvent();
            tracker.EndTransition();

            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));

            // --- Gameplay -> Result (GameOver) ---
            ch.onGameOver.RaiseEvent();
            Assert.That(tracker.TryBeginTransition("Result"), Is.True);
            ch.onScreenTransitionClosed.RaiseEvent();
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Result");
            ch.onSceneLoadCompleted.RaiseEvent();
            tracker.EndTransition();

            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.GameOver));
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));

            // --- Result -> Retry -> Gameplay ---
            ch.onResultRetrySelected.RaiseEvent();
            Assert.That(tracker.TryBeginTransition("Gameplay"), Is.True);
            ch.onScreenTransitionClosed.RaiseEvent();
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Gameplay");
            ch.onSceneLoadCompleted.RaiseEvent();
            tracker.EndTransition();

            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage));
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Gameplay"));

            // --- Gameplay -> Boss (immediate) ---
            ch.onBossTriggerReached.RaiseEvent();
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Boss));
            // tracker stays on Gameplay scene (no scene change for boss)

            // --- Boss -> Result (Clear) ---
            ch.onBossDefeated.RaiseEvent();
            Assert.That(tracker.TryBeginTransition("Result"), Is.True);
            ch.onScreenTransitionClosed.RaiseEvent();
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Result");
            ch.onSceneLoadCompleted.RaiseEvent();
            tracker.EndTransition();

            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Result));
            Assert.That(state.ResultTypeVar.Value, Is.EqualTo((int)GameResultType.Clear));
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Result"));

            // --- Result -> Title ---
            ch.onResultBackToTitleSelected.RaiseEvent();
            Assert.That(tracker.TryBeginTransition("Title"), Is.True);
            ch.onScreenTransitionClosed.RaiseEvent();
            tracker.BeginUnloadCurrent();
            tracker.HandleUnloadCompleted();
            tracker.HandleLoadCompleted("Title");
            ch.onSceneLoadCompleted.RaiseEvent();
            tracker.EndTransition();

            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Title));
            Assert.That(tracker.LoadedContentSceneName, Is.EqualTo("Title"));
            Assert.That(tracker.IsTransitioning, Is.False);
        }

        #endregion

        #region Scenario: Initial Load and Transition Open

        [Test]
        public void InitialLoad_ShouldSkipTransitionOpen()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened,
                ch.onTutorialCompleted);

            state.TransitionTo(GamePhase.Title);

            bool transitionOpened = false;
            ch.onScreenTransitionOpened.OnEventRaised += () => transitionOpened = true;

            // Act - fire scene load completed as if the initial Title scene loaded
            ch.onSceneLoadCompleted.RaiseEvent();

            // Assert - with isInitialLoad=true, Open should NOT be triggered
            Assert.That(transitionOpened, Is.False,
                "Initial load should skip transition open animation.");
            Assert.That(state.IsInitialLoad, Is.False,
                "isInitialLoad should be set to false after first scene load.");
        }

        [Test]
        public void SubsequentLoad_ShouldTriggerTransitionOpen()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened,
                ch.onTutorialCompleted);

            state.TransitionTo(GamePhase.Title);

            // Consume initial load
            ch.onSceneLoadCompleted.RaiseEvent();

            bool transitionOpened = false;
            ch.onScreenTransitionOpened.OnEventRaised += () => transitionOpened = true;

            // Act - fire scene load completed again (subsequent load)
            ch.onSceneLoadCompleted.RaiseEvent();

            // Assert - this time Open should be triggered
            Assert.That(transitionOpened, Is.True,
                "Subsequent scene load should trigger transition open animation.");
        }

        #endregion

        #region Scenario: Tutorial Completed Flow

        [Test]
        public void TutorialCompleted_ShouldSetPendingPhaseToTitle()
        {
            // Arrange
            var ch = CreateAllChannels();
            var state = CreateFlowState(
                ch.onGameOver, ch.onTitleStartSelected, ch.onBossTriggerReached,
                ch.onBossDefeated, ch.onResultRetrySelected, ch.onResultBackToTitleSelected,
                ch.onScreenTransitionClosed, ch.onSceneLoadCompleted, ch.onScreenTransitionOpened,
                ch.onTutorialCompleted);

            state.TransitionTo(GamePhase.Stage);

            // Act - fire tutorial completed
            ch.onTutorialCompleted.RaiseEvent();

            // Assert - pendingPhase should be Title, but currentPhase should NOT change yet
            Assert.That(state.PendingPhase, Is.EqualTo(GamePhase.Title),
                "onTutorialCompleted should set pendingPhase to Title.");
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Stage),
                "Phase should NOT change until transition closes.");

            // Act - simulate transition close completing
            ch.onScreenTransitionClosed.RaiseEvent();

            // Assert - now the phase should change to Title
            Assert.That(state.CurrentPhase, Is.EqualTo(GamePhase.Title),
                "After transition close, phase should change to Title.");
            Assert.That(state.GamePhaseVar.Value, Is.EqualTo((int)GamePhase.Title));
        }

        #endregion

        #region Scenario: Transition Color by Polarity

        [Test]
        public void TransitionColor_WhitePolarity_ShouldUseWhiteColor()
        {
            // Arrange
            int polarity = 0; // White

            // Act
            var color = TransitionColorHelper.GetColor(polarity);

            // Assert
            Assert.That(color.r, Is.EqualTo(0.941f).Within(0.001f));
            Assert.That(color.g, Is.EqualTo(0.933f).Within(0.001f));
            Assert.That(color.b, Is.EqualTo(0.902f).Within(0.001f));
            Assert.That(color.a, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void TransitionColor_BlackPolarity_ShouldUseDarkColor()
        {
            // Arrange
            int polarity = 1; // Black

            // Act
            var color = TransitionColorHelper.GetColor(polarity);

            // Assert
            Assert.That(color.r, Is.EqualTo(0.102f).Within(0.001f));
            Assert.That(color.g, Is.EqualTo(0.102f).Within(0.001f));
            Assert.That(color.b, Is.EqualTo(0.180f).Within(0.001f));
            Assert.That(color.a, Is.EqualTo(1f).Within(0.001f));
        }

        #endregion
    }
}
