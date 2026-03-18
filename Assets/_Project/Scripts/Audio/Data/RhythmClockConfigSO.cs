using UnityEngine;

namespace Action002.Audio.Data
{
    [CreateAssetMenu(fileName = "RhythmClockConfig", menuName = "Action002/Rhythm Clock Config")]
    public class RhythmClockConfigSO : ScriptableObject
    {
        [SerializeField] private float bpm = 120f;
        [SerializeField] private double startOffset;

        public float Bpm => bpm;
        public double StartOffset => startOffset;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bpm <= 0f)
            {
                bpm = 120f;
                Debug.LogWarning($"[{GetType().Name}] BPM must be positive. Reset to 120.", this);
            }
        }
#endif
    }
}
