using UnityEngine;

namespace Action002.Core.Save
{
    [CreateAssetMenu(fileName = "FirstPlayFlagRepository", menuName = "Action002/Save/FirstPlayFlagRepository")]
    public class FirstPlayFlagRepository : ScriptableObject
    {
        private const string KEY = "HasCompletedAwakeningTutorial";

        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt(KEY, 0) == 1;
        }

        public void SaveTutorialCompleted()
        {
            PlayerPrefs.SetInt(KEY, 1);
            PlayerPrefs.Save();
        }

        public void ClearTutorialFlag()
        {
            PlayerPrefs.DeleteKey(KEY);
            PlayerPrefs.Save();
        }
    }
}
