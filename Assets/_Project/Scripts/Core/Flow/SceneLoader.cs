using UnityEngine;
using UnityEngine.SceneManagement;
using Tang3cko.ReactiveSO;

namespace Action002.Core.Flow
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onSceneLoadCompleted;

        private SceneTransitionTracker tracker = new SceneTransitionTracker();

        public SceneTransitionTracker Tracker => tracker;

        public void LoadScene(string targetSceneName)
        {
            if (!tracker.TryBeginTransition(targetSceneName))
                return;

            if (tracker.HasLoadedScene())
            {
                string currentScene = tracker.LoadedContentSceneName;
                tracker.BeginUnloadCurrent();
                var unloadOp = SceneManager.UnloadSceneAsync(currentScene);
                if (unloadOp != null)
                {
                    unloadOp.completed += _ => OnUnloadCompleted(targetSceneName);
                }
                else
                {
                    // UnloadSceneAsync returned null - scene doesn't exist or can't be unloaded
                    // Still proceed to load the target
                    tracker.HandleUnloadCompleted();
                    LoadSceneAdditive(targetSceneName);
                }
            }
            else
            {
                LoadSceneAdditive(targetSceneName);
            }
        }

        private void OnUnloadCompleted(string targetSceneName)
        {
            tracker.HandleUnloadCompleted();
            LoadSceneAdditive(targetSceneName);
        }

        private void LoadSceneAdditive(string targetSceneName)
        {
            var op = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to load scene '{targetSceneName}' from {gameObject.name}.", this);
                tracker.AbortTransition();
                return;
            }

            op.completed += _ =>
            {
                Scene loadedScene = SceneManager.GetSceneByName(targetSceneName);
                if (loadedScene.IsValid())
                    SceneManager.SetActiveScene(loadedScene);

                tracker.HandleLoadCompleted(targetSceneName);
                tracker.EndTransition();
                onSceneLoadCompleted?.RaiseEvent();
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onSceneLoadCompleted == null) Debug.LogWarning($"[{GetType().Name}] onSceneLoadCompleted not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
