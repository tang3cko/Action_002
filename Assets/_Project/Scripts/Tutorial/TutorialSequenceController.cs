using System.Collections;
using UnityEngine;
using Action002.Input;
using Tang3cko.ReactiveSO;

namespace Action002.Tutorial
{
    public class TutorialSequenceController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("References")]
        [SerializeField] private SpriteRenderer effectSprite;

        [Header("Event")]
        [SerializeField] private VoidEventChannelSO onTutorialCompleted;

        [Header("Settings")]
        [SerializeField] private float[] expansionRates = { 0.4f, 0.7f, 1.0f };
        [SerializeField] private float expandDuration = 0.5f;
        [SerializeField] private float shrinkDuration = 0.3f;
        [SerializeField] private float maxScale = 60f;

        private int currentStep;
        private bool isAnimating;
        private bool isSubscribed;
        private Coroutine animationCoroutine;

        public void BeginSequence()
        {
            currentStep = 0;
            isAnimating = false;

            if (effectSprite != null)
            {
                effectSprite.transform.localScale = Vector3.zero;
                effectSprite.gameObject.SetActive(true);
            }

            if (inputReader != null && !isSubscribed)
            {
                inputReader.OnSwitchPolarityEvent += HandleSwitchPolarity;
                isSubscribed = true;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null && isSubscribed)
            {
                inputReader.OnSwitchPolarityEvent -= HandleSwitchPolarity;
                isSubscribed = false;
            }

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            isAnimating = false;
        }

        private void HandleSwitchPolarity()
        {
            if (isAnimating)
                return;

            if (currentStep >= expansionRates.Length)
                return;

            if (effectSprite == null)
                return;

            float targetRate = expansionRates[currentStep];
            animationCoroutine = StartCoroutine(ExpandAndShrinkCoroutine(targetRate));
        }

        private IEnumerator ExpandAndShrinkCoroutine(float targetRate)
        {
            isAnimating = true;

            if (effectSprite == null)
            {
                isAnimating = false;
                animationCoroutine = null;
                yield break;
            }

            float targetScale = targetRate * maxScale;

            // Expand — EaseOutQuad
            float elapsed = 0f;
            while (elapsed < expandDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / expandDuration);
                float eased = 1f - (1f - t) * (1f - t);
                float scale = eased * targetScale;
                effectSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            effectSprite.transform.localScale = new Vector3(targetScale, targetScale, 1f);

            if (targetRate >= 1.0f)
            {
                // Full-screen reached — tutorial complete
                if (onTutorialCompleted != null)
                    onTutorialCompleted.RaiseEvent();

                isAnimating = false;
                animationCoroutine = null;
                yield break;
            }

            // Shrink — EaseInQuad
            elapsed = 0f;
            float startScale = targetScale;
            while (elapsed < shrinkDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / shrinkDuration);
                float eased = t * t;
                float scale = Mathf.Lerp(startScale, 0f, eased);
                effectSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            effectSprite.transform.localScale = Vector3.zero;

            currentStep++;
            isAnimating = false;
            animationCoroutine = null;
        }

        public void EndSequence()
        {
            if (inputReader != null && isSubscribed)
            {
                inputReader.OnSwitchPolarityEvent -= HandleSwitchPolarity;
                isSubscribed = false;
            }

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            if (effectSprite != null)
            {
                effectSprite.transform.localScale = Vector3.zero;
                effectSprite.gameObject.SetActive(false);
            }

            isAnimating = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (inputReader == null)
                Debug.LogWarning($"[{GetType().Name}] inputReader not assigned on {gameObject.name}.", this);
            if (effectSprite == null)
                Debug.LogWarning($"[{GetType().Name}] effectSprite not assigned on {gameObject.name}.", this);
            if (onTutorialCompleted == null)
                Debug.LogWarning($"[{GetType().Name}] onTutorialCompleted not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
