namespace Action002.Core.Flow
{
    public class SceneTransitionTracker
    {
        private bool isTransitioning;
        private string loadedContentSceneName;
        private string pendingSceneName;
        private string unloadingSceneName;

        public bool IsTransitioning => isTransitioning;
        public string LoadedContentSceneName => loadedContentSceneName;

        public bool TryBeginTransition(string targetSceneName)
        {
            if (isTransitioning) return false;
            isTransitioning = true;
            pendingSceneName = targetSceneName;
            return true;
        }

        public void SetLoadedScene(string sceneName)
        {
            loadedContentSceneName = sceneName;
        }

        public bool HasLoadedScene()
        {
            return !string.IsNullOrEmpty(loadedContentSceneName);
        }

        public string ConsumePendingLoad()
        {
            var pending = pendingSceneName;
            pendingSceneName = null;
            return pending;
        }

        public void BeginUnloadCurrent()
        {
            unloadingSceneName = loadedContentSceneName;
            loadedContentSceneName = null;
        }

        public bool ShouldHandleUnloadEvent(string sceneName)
        {
            return unloadingSceneName == sceneName;
        }

        public string HandleUnloadCompleted()
        {
            var unloaded = unloadingSceneName;
            unloadingSceneName = null;
            return unloaded;
        }

        public bool ShouldHandleLoadEvent(string sceneName)
        {
            // Only handle if this is the scene we're expecting to load
            if (!isTransitioning) return false;
            return pendingSceneName == sceneName;
        }

        public void HandleLoadCompleted(string sceneName)
        {
            loadedContentSceneName = sceneName;
            pendingSceneName = null;
        }

        public void EndTransition()
        {
            isTransitioning = false;
            pendingSceneName = null;
            unloadingSceneName = null;
        }

        public void AbortTransition()
        {
            isTransitioning = false;
            pendingSceneName = null;
            unloadingSceneName = null;
        }
    }
}
