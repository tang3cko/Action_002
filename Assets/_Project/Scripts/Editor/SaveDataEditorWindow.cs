#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Action002.Core.Save;

namespace Action002.Editor
{
    public class SaveDataEditorWindow : EditorWindow
    {
        private PlayerPrefsSaveDataRepository repository;
        private SaveData current;
        private bool loaded;

        [MenuItem("Action002/Save Data Viewer")]
        private static void Open()
        {
            GetWindow<SaveDataEditorWindow>("Save Data");
        }

        private void OnEnable()
        {
            Reload();
        }

        private void Reload()
        {
            repository = new PlayerPrefsSaveDataRepository();
            current = repository.Load();
            loaded = true;
            Repaint();
        }

        private void OnGUI()
        {
            if (!loaded)
            {
                Reload();
            }

            EditorGUILayout.LabelField("Save Data Viewer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Version", current.Version);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layer 1", EditorStyles.boldLabel);
            EditorGUILayout.IntField("High Score", current.HighScore);
            EditorGUILayout.Toggle("Tutorial Completed", current.TutorialCompleted);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layer 3 (Statistics)", EditorStyles.boldLabel);
            EditorGUILayout.IntField("Best Combo", current.BestCombo);
            EditorGUILayout.IntField("Total Kills", current.TotalKills);
            EditorGUILayout.IntField("Total Absorptions", current.TotalAbsorptions);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reload"))
            {
                Reload();
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Reset All"))
            {
                if (EditorUtility.DisplayDialog(
                        "Reset Save Data",
                        "All save data will be deleted. This cannot be undone.",
                        "Reset",
                        "Cancel"))
                {
                    PlayerPrefs.DeleteKey("Action002_SaveData");
                    PlayerPrefs.DeleteKey("HasCompletedAwakeningTutorial");
                    PlayerPrefs.Save();
                    Reload();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
