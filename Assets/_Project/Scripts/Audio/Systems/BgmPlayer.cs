using UnityEngine;
using Action002.Audio.Data;

namespace Action002.Audio.Systems
{
    public class BgmPlayer : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;

        [Header("Config")]
        [SerializeField] private RhythmClockConfigSO config;

        public void PlayBgm()
        {
            if (bgmSource != null && config != null)
                bgmSource.PlayScheduled(AudioSettings.dspTime + config.StartOffset);
        }

        public void StopBgm()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bgmSource == null) Debug.LogWarning($"[{GetType().Name}] bgmSource not assigned on {gameObject.name}.", this);
            if (config == null) Debug.LogWarning($"[{GetType().Name}] config not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
