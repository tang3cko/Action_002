namespace Action002.Core.Flow
{
    /// <summary>
    /// Side-effect interface that the thin MonoBehaviour wrapper implements.
    /// All Unity-dependent operations for gameplay startup are expressed here
    /// so that <see cref="GameplayStartupLogic"/> remains a pure C# class.
    /// </summary>
    public interface IGameplayStartupActions
    {
        void DisablePlayerInput();
        void EnablePlayerInput();
        void ResetForNewRun();
        bool StartClock();
        void SetRunning(bool running);
        void LogStartupError(string message);
    }
}
