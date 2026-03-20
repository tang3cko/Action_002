namespace Action002.Core.Flow
{
    /// <summary>
    /// Side-effect interface that the thin MonoBehaviour wrapper implements.
    /// All Unity-dependent operations are expressed here so that GameFlowLogic
    /// remains a pure C# class with no Unity dependencies.
    /// </summary>
    public interface IGameFlowActions
    {
        void LoadScene(string sceneName);
        void CloseTransition();
        void CloseTransitionWithOrigin(float screenX, float screenY);
        void ConvergeTransitionToPlayer();
        void ClearTransitionImmediate();
        void RaiseBossPhaseRequested();
        void RaiseGamePhaseChanged(int phase);
        void SetGamePhaseVariable(int phase);
        void SetResultTypeVariable(int resultType);
        void SaveTutorialCompleted();
    }

    /// <summary>
    /// Pure C# state machine that owns all game-flow decisions.
    /// Instantiable with <c>new</c>; no MonoBehaviour dependency.
    /// Communicates side-effects through <see cref="IGameFlowActions"/>.
    /// </summary>
    public class GameFlowLogic
    {
        public GamePhase CurrentPhase { get; private set; }
        public GamePhase PendingPhase { get; private set; }
        public GameResultType PendingResultType { get; private set; }

        private float pendingOriginX;
        private float pendingOriginY;
        private bool hasPendingTransitionOrigin;

        private readonly IGameFlowActions actions;

        public GameFlowLogic(IGameFlowActions actions)
        {
            this.actions = actions;
        }

        /// <summary>
        /// Called once at startup to set the initial phase and load the title scene.
        /// </summary>
        public void Initialize()
        {
            TransitionTo(GamePhase.Title);
            PendingPhase = CurrentPhase;
            actions.LoadScene("Title");
        }

        // --- Event Handlers ---

        public void HandleGameOver()
        {
            PendingPhase = GamePhase.Result;
            PendingResultType = GameResultType.GameOver;
            actions.ConvergeTransitionToPlayer();
        }

        public void HandleTutorialCompleted()
        {
            PendingPhase = GamePhase.Title;
            actions.SaveTutorialCompleted();
            actions.CloseTransition();
        }

        public void HandleTitleStartTransitionOriginSelected(float screenX, float screenY)
        {
            pendingOriginX = screenX;
            pendingOriginY = screenY;
            hasPendingTransitionOrigin = true;
        }

        public void HandleTitleStartSelected()
        {
            PendingPhase = GamePhase.Stage;

            if (hasPendingTransitionOrigin)
                actions.CloseTransitionWithOrigin(pendingOriginX, pendingOriginY);
            else
                actions.CloseTransition();

            ClearPendingTransitionOrigin();
        }

        public void HandleBossTriggerReached()
        {
            actions.RaiseBossPhaseRequested();
            TransitionTo(GamePhase.Boss);
        }

        public void HandleBossDefeated()
        {
            PendingPhase = GamePhase.Result;
            PendingResultType = GameResultType.Clear;
            actions.CloseTransition();
        }

        public void HandleResultRetrySelected()
        {
            PendingPhase = GamePhase.Stage;
            actions.CloseTransition();
        }

        public void HandleResultBackToTitleSelected()
        {
            PendingPhase = GamePhase.Title;
            actions.CloseTransition();
        }

        public void HandleTransitionClosed()
        {
            TransitionTo(PendingPhase);

            switch (PendingPhase)
            {
                case GamePhase.Stage:
                    actions.LoadScene("Gameplay");
                    break;
                case GamePhase.Title:
                    actions.LoadScene("Title");
                    break;
                case GamePhase.Result:
                    actions.SetResultTypeVariable((int)PendingResultType);
                    actions.LoadScene("Result");
                    break;
            }
        }

        public void HandleTransitionOpened()
        {
            // intentionally empty — reserved for future use
        }

        public void HandleSceneLoadCompleted()
        {
            actions.ClearTransitionImmediate();
        }

        // --- Phase Transition ---

        public void TransitionTo(GamePhase next)
        {
            CurrentPhase = next;
            actions.SetGamePhaseVariable((int)CurrentPhase);
            actions.RaiseGamePhaseChanged((int)CurrentPhase);
        }

        private void ClearPendingTransitionOrigin()
        {
            pendingOriginX = 0f;
            pendingOriginY = 0f;
            hasPendingTransitionOrigin = false;
        }
    }
}
