#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Action002.Editor
{
    /// <summary>
    /// Enforces Core scene as playModeStartScene in Editor.
    /// Saves the current scene before play and restores it after.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorBootstrapEnforcer
    {
        private const string CORE_SCENE_PATH = "Assets/_Project/Scenes/Core.unity";
        private const string CORE_SCENE_NAME = "Core";

        // EditorPrefs keys
        private const string ENABLED_KEY = "Action002_UseCoreOnPlay";
        private const string PREVIOUS_SCENE_KEY = "Action002_PreviousScene";

        // PlayerPrefs key (for runtime communication)
        public const string RUNTIME_PREVIOUS_SCENE_KEY = "Action002_EditorPreviousScene";

        // Menu paths
        private const string MENU_PATH = "Action002/Use Core Scene on Play";

        static EditorBootstrapEnforcer()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static bool IsUseCoreOnPlay
        {
            get => EditorPrefs.GetBool(ENABLED_KEY, true);
            set => EditorPrefs.SetBool(ENABLED_KEY, value);
        }

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(PREVIOUS_SCENE_KEY, string.Empty);
            set => EditorPrefs.SetString(PREVIOUS_SCENE_KEY, value);
        }

        [MenuItem(MENU_PATH)]
        private static void ToggleCoreOnPlay()
        {
            IsUseCoreOnPlay = !IsUseCoreOnPlay;
            Debug.Log($"[EditorBootstrapEnforcer] Core on Play: {(IsUseCoreOnPlay ? "Enabled" : "Disabled")}");
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ToggleCoreOnPlayValidate()
        {
            Menu.SetChecked(MENU_PATH, IsUseCoreOnPlay);
            return true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!IsUseCoreOnPlay)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    HandleExitingEditMode();
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    HandleEnteredEditMode();
                    break;
            }
        }

        private static void HandleExitingEditMode()
        {
            var coreScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(CORE_SCENE_PATH);
            if (coreScene == null)
            {
                Debug.LogError($"[EditorBootstrapEnforcer] Core scene not found at {CORE_SCENE_PATH}");
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            string activeScenePath = activeScene.path;
            string activeSceneName = activeScene.name;

            if (activeSceneName != CORE_SCENE_NAME && !string.IsNullOrEmpty(activeScenePath))
            {
                PreviousScene = activeScenePath;
                PlayerPrefs.SetString(RUNTIME_PREVIOUS_SCENE_KEY, activeSceneName);
                PlayerPrefs.Save();
                Debug.Log($"[EditorBootstrapEnforcer] Saved previous scene: {activeSceneName}");
            }
            else
            {
                PreviousScene = string.Empty;
                PlayerPrefs.DeleteKey(RUNTIME_PREVIOUS_SCENE_KEY);
                PlayerPrefs.Save();
            }

            EditorSceneManager.playModeStartScene = coreScene;
        }

        private static void HandleEnteredEditMode()
        {
            PlayerPrefs.DeleteKey(RUNTIME_PREVIOUS_SCENE_KEY);
            PlayerPrefs.Save();

            string previousScenePath = PreviousScene;
            if (!string.IsNullOrEmpty(previousScenePath))
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScenePath);
                if (sceneAsset != null)
                {
                    EditorSceneManager.OpenScene(previousScenePath);
                    Debug.Log($"[EditorBootstrapEnforcer] Restored previous scene: {previousScenePath}");
                }

                PreviousScene = string.Empty;
            }
        }

        [MenuItem("Action002/Clear Play Mode Start Scene")]
        private static void ClearPlayModeStartScene()
        {
            EditorSceneManager.playModeStartScene = null;
            PreviousScene = string.Empty;
            PlayerPrefs.DeleteKey(RUNTIME_PREVIOUS_SCENE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[EditorBootstrapEnforcer] Play mode start scene cleared.");
        }
    }
}
#endif
