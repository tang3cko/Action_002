using UnityEngine;
using Action002.Core.Save;
using Tang3cko.ReactiveSO;

namespace Action002.Core.Flow
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RunSessionController runSessionController;
        [SerializeField] private FirstPlayFlagRepository firstPlayFlagRepository;

        [Header("Events (subscribe)")]
        [SerializeField] private VoidEventChannelSO onGameOver;
        [SerializeField] private VoidEventChannelSO onTutorialCompleted;
        [SerializeField] private VoidEventChannelSO onTitleStartSelected;
        [SerializeField] private VoidEventChannelSO onBossTriggerReached;
        [SerializeField] private VoidEventChannelSO onBossDefeated;
        [SerializeField] private VoidEventChannelSO onResultRetrySelected;
        [SerializeField] private VoidEventChannelSO onResultBackToTitleSelected;

        [Header("Variables (write)")]
        [SerializeField] private IntVariableSO gamePhaseVar;

        private GamePhase currentPhase;

        private void Awake()
        {
            if (firstPlayFlagRepository != null && firstPlayFlagRepository.HasCompletedTutorial())
            {
                // チュートリアル完了済み → Title（実画面未実装のためStageへフォールバック）
                TransitionTo(GamePhase.Title);
                TransitionTo(GamePhase.Stage);
            }
            else
            {
                // 未完了 → Tutorial（実画面未実装のためStageへフォールバック）
                TransitionTo(GamePhase.Tutorial);
                TransitionTo(GamePhase.Stage);
            }
        }

        private void OnEnable()
        {
            if (onGameOver != null)
                onGameOver.OnEventRaised += HandleGameOver;
            if (onTutorialCompleted != null)
                onTutorialCompleted.OnEventRaised += HandleTutorialCompleted;
            if (onTitleStartSelected != null)
                onTitleStartSelected.OnEventRaised += HandleTitleStartSelected;
            if (onBossTriggerReached != null)
                onBossTriggerReached.OnEventRaised += HandleBossTriggerReached;
            if (onBossDefeated != null)
                onBossDefeated.OnEventRaised += HandleBossDefeated;
            if (onResultRetrySelected != null)
                onResultRetrySelected.OnEventRaised += HandleResultRetrySelected;
            if (onResultBackToTitleSelected != null)
                onResultBackToTitleSelected.OnEventRaised += HandleResultBackToTitleSelected;
        }

        private void OnDisable()
        {
            if (onGameOver != null)
                onGameOver.OnEventRaised -= HandleGameOver;
            if (onTutorialCompleted != null)
                onTutorialCompleted.OnEventRaised -= HandleTutorialCompleted;
            if (onTitleStartSelected != null)
                onTitleStartSelected.OnEventRaised -= HandleTitleStartSelected;
            if (onBossTriggerReached != null)
                onBossTriggerReached.OnEventRaised -= HandleBossTriggerReached;
            if (onBossDefeated != null)
                onBossDefeated.OnEventRaised -= HandleBossDefeated;
            if (onResultRetrySelected != null)
                onResultRetrySelected.OnEventRaised -= HandleResultRetrySelected;
            if (onResultBackToTitleSelected != null)
                onResultBackToTitleSelected.OnEventRaised -= HandleResultBackToTitleSelected;
        }

        // --- Event Handlers ---

        private void HandleGameOver()
        {
            if (runSessionController != null)
                runSessionController.StopRunLoop();
            TransitionTo(GamePhase.Result);
        }

        private void HandleTutorialCompleted()
        {
            if (firstPlayFlagRepository == null)
                return;

            firstPlayFlagRepository.SaveTutorialCompleted();
            TransitionTo(GamePhase.Title);
        }

        private void HandleTitleStartSelected()
        {
            TransitionTo(GamePhase.Stage);
        }

        private void HandleBossTriggerReached()
        {
            if (runSessionController != null)
                runSessionController.PrepareBossPhase();
            TransitionTo(GamePhase.Boss);
        }

        private void HandleBossDefeated()
        {
            if (runSessionController != null)
                runSessionController.StopRunLoop();
            TransitionTo(GamePhase.Result);
        }

        private void HandleResultRetrySelected()
        {
            if (runSessionController != null)
            {
                runSessionController.ResetRun();
                runSessionController.StartRun();
            }
            TransitionTo(GamePhase.Stage);
        }

        private void HandleResultBackToTitleSelected()
        {
            if (runSessionController != null)
                runSessionController.CleanupRunState();
            TransitionTo(GamePhase.Title);
        }

        // --- Phase Transition ---

        private void TransitionTo(GamePhase next)
        {
            currentPhase = next;

            if (gamePhaseVar != null)
                gamePhaseVar.Value = (int)currentPhase;

            if (runSessionController != null)
                runSessionController.SetPhase(currentPhase);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (runSessionController == null) Debug.LogWarning($"[{GetType().Name}] runSessionController not assigned on {gameObject.name}.", this);
            if (firstPlayFlagRepository == null) Debug.LogWarning($"[{GetType().Name}] firstPlayFlagRepository not assigned on {gameObject.name}.", this);
            if (onGameOver == null) Debug.LogWarning($"[{GetType().Name}] onGameOver not assigned on {gameObject.name}.", this);
            if (onTutorialCompleted == null) Debug.LogWarning($"[{GetType().Name}] onTutorialCompleted not assigned on {gameObject.name}.", this);
            if (onTitleStartSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartSelected not assigned on {gameObject.name}.", this);
            if (onBossTriggerReached == null) Debug.LogWarning($"[{GetType().Name}] onBossTriggerReached not assigned on {gameObject.name}.", this);
            if (onBossDefeated == null) Debug.LogWarning($"[{GetType().Name}] onBossDefeated not assigned on {gameObject.name}.", this);
            if (onResultRetrySelected == null) Debug.LogWarning($"[{GetType().Name}] onResultRetrySelected not assigned on {gameObject.name}.", this);
            if (onResultBackToTitleSelected == null) Debug.LogWarning($"[{GetType().Name}] onResultBackToTitleSelected not assigned on {gameObject.name}.", this);
            if (gamePhaseVar == null) Debug.LogWarning($"[{GetType().Name}] gamePhaseVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
