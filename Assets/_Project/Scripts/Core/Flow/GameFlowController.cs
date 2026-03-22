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

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO scoreVar;
        [SerializeField] private IntVariableSO maxComboVar;
        [SerializeField] private IntVariableSO runKillCountVar;
        [SerializeField] private IntVariableSO runAbsorptionCountVar;

        [Header("Variables (write)")]
        [SerializeField] private IntVariableSO gamePhaseVar;
        [SerializeField] private IntVariableSO resultTypeVar;

        private GameFlowLogic logic;
        private SaveDataService saveDataService;

        private GameFlowLogic Logic => logic ?? (logic = new GameFlowLogic(this));

        private SaveDataService SaveService =>
            saveDataService ?? (saveDataService = CreateSaveDataService());

        public void InjectSaveDataService(SaveDataService service)
        {
            saveDataService = service;
        }

        private static SaveDataService CreateSaveDataService()
        {
            return new SaveDataService(new PlayerPrefsSaveDataRepository());
        }

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

        private void Start()
        {
            Logic.Initialize();
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

        void IGameFlowActions.ConvergeTransitionToPlayer()
        {
            if (screenTransitionController == null) return;

            if (playerPositionVar != null)
                screenTransitionController.Converge(playerPositionVar.Value);
            else
                screenTransitionController.Close();
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
            SaveService.MarkTutorialCompleted();
        }

        void IGameFlowActions.CommitRunResult()
        {
            int finalScore = scoreVar != null ? scoreVar.Value : 0;
            int maxCombo = maxComboVar != null ? maxComboVar.Value : 0;
            int killCount = runKillCountVar != null ? runKillCountVar.Value : 0;
            int absorptionCount = runAbsorptionCountVar != null ? runAbsorptionCountVar.Value : 0;

            SaveService.ApplyRunResult(finalScore, maxCombo, killCount, absorptionCount);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneLoader == null) Debug.LogWarning($"[{GetType().Name}] sceneLoader not assigned on {gameObject.name}.", this);
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
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (scoreVar == null) Debug.LogWarning($"[{GetType().Name}] scoreVar not assigned on {gameObject.name}.", this);
            if (maxComboVar == null) Debug.LogWarning($"[{GetType().Name}] maxComboVar not assigned on {gameObject.name}.", this);
            if (runKillCountVar == null) Debug.LogWarning($"[{GetType().Name}] runKillCountVar not assigned on {gameObject.name}.", this);
            if (runAbsorptionCountVar == null) Debug.LogWarning($"[{GetType().Name}] runAbsorptionCountVar not assigned on {gameObject.name}.", this);
            if (gamePhaseVar == null) Debug.LogWarning($"[{GetType().Name}] gamePhaseVar not assigned on {gameObject.name}.", this);
            if (resultTypeVar == null) Debug.LogWarning($"[{GetType().Name}] resultTypeVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
