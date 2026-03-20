namespace Action002.Core.Flow
{
    /// <summary>
    /// Pure C# startup coordinator for the gameplay scene.
    /// Instantiable with <c>new</c>; no MonoBehaviour dependency.
    /// Communicates side-effects through <see cref="IGameplayStartupActions"/>.
    /// </summary>
    public class GameplayStartupLogic
    {
        private readonly IGameplayStartupActions actions;

        public GameplayStartupLogic(IGameplayStartupActions actions)
        {
            this.actions = actions;
        }

        /// <summary>
        /// Executes the gameplay startup sequence:
        /// DisablePlayerInput → ResetForNewRun → StartClock → EnablePlayerInput → SetRunning(true).
        /// If StartClock fails, player input remains disabled and the game loop is not started.
        /// </summary>
        public void Execute()
        {
            actions.DisablePlayerInput();
            actions.ResetForNewRun();

            bool clockStarted = actions.StartClock();
            if (!clockStarted)
            {
                actions.LogStartupError("[GameplayStartupLogic] StartClock failed. Gameplay will not start.");
                return;
            }

            actions.EnablePlayerInput();
            actions.SetRunning(true);
        }
    }
}
