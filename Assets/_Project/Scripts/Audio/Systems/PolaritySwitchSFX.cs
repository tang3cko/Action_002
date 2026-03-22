using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Audio.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public class PolaritySwitchSFX : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioSource audioSource;

        [Header("Settings")]
        [SerializeField] private AudioClip switchClip;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        private void OnEnable()
        {
            if (onPolarityChanged == null)
            {
                return;
            }

            onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            if (onPolarityChanged == null)
            {
                return;
            }

            onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void HandlePolarityChanged(int newPolarity)
        {
            if (audioSource == null || switchClip == null)
            {
                return;
            }

            audioSource.PlayOneShot(switchClip);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                Debug.LogWarning($"[{GetType().Name}] audioSource not assigned on {gameObject.name}.", this);
            }

            if (switchClip == null)
            {
                Debug.LogWarning($"[{GetType().Name}] switchClip not assigned on {gameObject.name}.", this);
            }

            if (onPolarityChanged == null)
            {
                Debug.LogWarning($"[{GetType().Name}] onPolarityChanged not assigned on {gameObject.name}.", this);
            }
        }
#endif
    }
}
