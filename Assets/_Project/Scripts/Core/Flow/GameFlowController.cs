using UnityEngine;
using Action002.Core.Save;
using Action002.Visual;
using Tang3cko.ReactiveSO;

namespace Action002.Core.Flow
{
    public class GameFlowController : MonoBehaviour, IGameFlowActions
    {
        [Header("Dependencies")]
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private FirstPlayFlagRepository firstPlayFlagRepository;
        [SerializeField] private ScreenTransitionController screenTransitionController;

        [Header("Events (subscribe)")]
        [SerializeField] private VoidEventChannelSO onGameOver;
        [SerializeField] private VoidEventChannelSO onTutorialCompleted;
        [SerializeField] private Vector2EventChannelSO onTitleStartTransitionOriginSelected;
        [SerializeField] private VoidEventChannelSO onTitleStartSelected;
        [SerializeField] private VoidEventChannelSO onBossTriggerReached;
        [SerializeField] private VoidEventChannelSO onBossDefeated;
        [SerializeField] private VoidEventChannelSO onResultRetrySelected;
        [SerializeField] private VoidEventChannelSO onResultBackToTitleSelected;
        [SerializeField] private VoidEventChannelSO onScreenTransitionClosed;
        [SerializeField] private VoidEventChannelSO onScreenTransitionOpened;
        [SerializeField] private VoidEventChannelSO onSceneLoadCompleted;

        [Header("Events (publish)")]
        [SerializeField] private IntEventChannelSO onGamePhaseChanged;
        [SerializeField] private VoidEventChannelSO onBossPhaseRequested;

        [Header("Variables (write)")]
        [SerializeField] private IntVariableSO gamePhaseVar;
        [SerializeField] private IntVariableSO resultTypeVar;

        private GameFlowLogic logic;

        private GameFlowLogic Logic => logic ?? (logic = new GameFlowLogic(this));

        private void OnEnable()
        {
            if (onGameOver != null)
                onGameOver.OnEventRaised += HandleGameOver;
            if (onTutorialCompleted != null)
                onTutorialCompleted.OnEventRaised += HandleTutorialCompleted;
            if (onTitleStartTransitionOriginSelected != null)
                onTitleStartTransitionOriginSelected.OnEventRaised += HandleTitleStartTransitionOriginSelected;
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
            if (onScreenTransitionClosed != null)
                onScreenTransitionClosed.OnEventRaised += HandleTransitionClosed;
            if (onScreenTransitionOpened != null)
                onScreenTransitionOpened.OnEventRaised += HandleTransitionOpened;
            if (onSceneLoadCompleted != null)
                onSceneLoadCompleted.OnEventRaised += HandleSceneLoadCompleted;
        }

        private void OnDisable()
        {
            if (onGameOver != null)
                onGameOver.OnEventRaised -= HandleGameOver;
            if (onTutorialCompleted != null)
                onTutorialCompleted.OnEventRaised -= HandleTutorialCompleted;
            if (onTitleStartTransitionOriginSelected != null)
                onTitleStartTransitionOriginSelected.OnEventRaised -= HandleTitleStartTransitionOriginSelected;
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
            if (onScreenTransitionClosed != null)
                onScreenTransitionClosed.OnEventRaised -= HandleTransitionClosed;
            if (onScreenTransitionOpened != null)
                onScreenTransitionOpened.OnEventRaised -= HandleTransitionOpened;
            if (onSceneLoadCompleted != null)
                onSceneLoadCompleted.OnEventRaised -= HandleSceneLoadCompleted;
        }

        private void Start()
        {
            Logic.Initialize();
        }

        // --- Event Handlers (delegate to logic) ---

        private void HandleGameOver()
        {
            Logic.HandleGameOver();
        }

        private void HandleTutorialCompleted()
        {
            Logic.HandleTutorialCompleted();
        }

        private void HandleTitleStartTransitionOriginSelected(Vector2 screenPosition)
        {
            Logic.HandleTitleStartTransitionOriginSelected(screenPosition.x, screenPosition.y);
        }

        private void HandleTitleStartSelected()
        {
            Logic.HandleTitleStartSelected();
        }

        private void HandleBossTriggerReached()
        {
            Logic.HandleBossTriggerReached();
        }

        private void HandleBossDefeated()
        {
            Logic.HandleBossDefeated();
        }

        private void HandleResultRetrySelected()
        {
            Logic.HandleResultRetrySelected();
        }

        private void HandleResultBackToTitleSelected()
        {
            Logic.HandleResultBackToTitleSelected();
        }

        private void HandleTransitionClosed()
        {
            Logic.HandleTransitionClosed();
        }

        private void HandleTransitionOpened()
        {
            Logic.HandleTransitionOpened();
        }

        private void HandleSceneLoadCompleted()
        {
            Logic.HandleSceneLoadCompleted();
        }

        // --- IGameFlowActions implementation ---

        void IGameFlowActions.LoadScene(string sceneName)
        {
            if (sceneLoader == null)
            {
                Debug.LogError($"[{GetType().Name}] sceneLoader not assigned on {gameObject.name}. Failed to load scene '{sceneName}'.", this);
                return;
            }

            sceneLoader.LoadScene(sceneName);
        }

        void IGameFlowActions.CloseTransition()
        {
            if (screenTransitionController != null)
                screenTransitionController.Close();
        }

        void IGameFlowActions.CloseTransitionWithOrigin(float screenX, float screenY)
        {
            if (screenTransitionController != null)
                screenTransitionController.Close(new Vector2(screenX, screenY));
        }

        void IGameFlowActions.ClearTransitionImmediate()
        {
            if (screenTransitionController != null)
                screenTransitionController.ClearImmediate();
        }

        void IGameFlowActions.RaiseBossPhaseRequested()
        {
            onBossPhaseRequested?.RaiseEvent();
        }

        void IGameFlowActions.RaiseGamePhaseChanged(int phase)
        {
            onGamePhaseChanged?.RaiseEvent(phase);
        }

        void IGameFlowActions.SetGamePhaseVariable(int phase)
        {
            if (gamePhaseVar != null)
                gamePhaseVar.Value = phase;
        }

        void IGameFlowActions.SetResultTypeVariable(int resultType)
        {
            if (resultTypeVar != null)
                resultTypeVar.Value = resultType;
        }

        void IGameFlowActions.SaveTutorialCompleted()
        {
            if (firstPlayFlagRepository != null)
            {
                firstPlayFlagRepository.SaveTutorialCompleted();
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] firstPlayFlagRepository not assigned on {gameObject.name}. Tutorial completion was not persisted.", this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneLoader == null) Debug.LogWarning($"[{GetType().Name}] sceneLoader not assigned on {gameObject.name}.", this);
            if (firstPlayFlagRepository == null) Debug.LogWarning($"[{GetType().Name}] firstPlayFlagRepository not assigned on {gameObject.name}.", this);
            if (screenTransitionController == null) Debug.LogWarning($"[{GetType().Name}] screenTransitionController not assigned on {gameObject.name}.", this);
            if (onGameOver == null) Debug.LogWarning($"[{GetType().Name}] onGameOver not assigned on {gameObject.name}.", this);
            if (onTutorialCompleted == null) Debug.LogWarning($"[{GetType().Name}] onTutorialCompleted not assigned on {gameObject.name}.", this);
            if (onTitleStartTransitionOriginSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartTransitionOriginSelected not assigned on {gameObject.name}.", this);
            if (onTitleStartSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartSelected not assigned on {gameObject.name}.", this);
            if (onBossTriggerReached == null) Debug.LogWarning($"[{GetType().Name}] onBossTriggerReached not assigned on {gameObject.name}.", this);
            if (onBossDefeated == null) Debug.LogWarning($"[{GetType().Name}] onBossDefeated not assigned on {gameObject.name}.", this);
            if (onResultRetrySelected == null) Debug.LogWarning($"[{GetType().Name}] onResultRetrySelected not assigned on {gameObject.name}.", this);
            if (onResultBackToTitleSelected == null) Debug.LogWarning($"[{GetType().Name}] onResultBackToTitleSelected not assigned on {gameObject.name}.", this);
            if (onScreenTransitionClosed == null) Debug.LogWarning($"[{GetType().Name}] onScreenTransitionClosed not assigned on {gameObject.name}.", this);
            if (onScreenTransitionOpened == null) Debug.LogWarning($"[{GetType().Name}] onScreenTransitionOpened not assigned on {gameObject.name}.", this);
            if (onSceneLoadCompleted == null) Debug.LogWarning($"[{GetType().Name}] onSceneLoadCompleted not assigned on {gameObject.name}.", this);
            if (onGamePhaseChanged == null) Debug.LogWarning($"[{GetType().Name}] onGamePhaseChanged not assigned on {gameObject.name}.", this);
            if (onBossPhaseRequested == null) Debug.LogWarning($"[{GetType().Name}] onBossPhaseRequested not assigned on {gameObject.name}.", this);
            if (gamePhaseVar == null) Debug.LogWarning($"[{GetType().Name}] gamePhaseVar not assigned on {gameObject.name}.", this);
            if (resultTypeVar == null) Debug.LogWarning($"[{GetType().Name}] resultTypeVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
