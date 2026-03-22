using System.Collections;
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class HitstopEffect : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        [Header("References")]
        [SerializeField] private Transform playerTransform;

        [Header("Settings")]
        [SerializeField] private float hitstopDuration = 0.05f;
        [SerializeField] private float hitstopTimeScale = 0.1f;
        [SerializeField] private float scalePulseAmount = 1.3f;
        [SerializeField] private float scalePulseDuration = 0.15f;

        private Coroutine hitstopCoroutine;
        private Coroutine scaleCoroutine;
        private float previousTimeScale = 1f;
        private Vector3 baseScale = Vector3.one;
        private bool baseScaleCaptured;

        private void OnEnable()
        {
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised -= HandlePolarityChanged;

            RestoreTimeScale();
            RestorePlayerScale();
        }

        private void HandlePolarityChanged(int polarity)
        {
            // Restore timeScale before starting new hitstop to avoid capturing mid-hitstop value
            if (hitstopCoroutine != null)
            {
                StopCoroutine(hitstopCoroutine);
                Time.timeScale = previousTimeScale;
            }
            hitstopCoroutine = StartCoroutine(HitstopCoroutine());

            if (playerTransform != null)
            {
                if (!baseScaleCaptured)
                {
                    baseScale = playerTransform.localScale;
                    baseScaleCaptured = true;
                }

                if (scaleCoroutine != null)
                    StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(ScalePulseCoroutine());
            }
        }

        private IEnumerator HitstopCoroutine()
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = hitstopTimeScale;
            float elapsed = 0f;
            while (elapsed < hitstopDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Time.timeScale = previousTimeScale;
            hitstopCoroutine = null;
        }

        private IEnumerator ScalePulseCoroutine()
        {
            Vector3 pulsedScale = baseScale * scalePulseAmount;

            float elapsed = 0f;
            while (elapsed < scalePulseDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / scalePulseDuration);
                float eased = 1f - (1f - t) * (1f - t);
                playerTransform.localScale = Vector3.Lerp(pulsedScale, baseScale, eased);
                yield return null;
            }

            playerTransform.localScale = baseScale;
            scaleCoroutine = null;
        }

        private void RestoreTimeScale()
        {
            if (hitstopCoroutine != null)
            {
                StopCoroutine(hitstopCoroutine);
                Time.timeScale = previousTimeScale;
                hitstopCoroutine = null;
            }
        }

        private void RestorePlayerScale()
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            if (playerTransform != null && baseScaleCaptured)
                playerTransform.localScale = baseScale;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onPolarityChanged == null)
                Debug.LogWarning($"[{GetType().Name}] onPolarityChanged not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
