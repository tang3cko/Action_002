using System.Collections;
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class ScreenTransitionController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Camera transitionCamera;

        [Header("References")]
        [SerializeField] private SpriteRenderer maskSprite;

        [Header("Settings")]
        [SerializeField] private float transitionDuration = 0.4f;
        [SerializeField] private float maxScale = 60f;

        [Header("Variables (read)")]
        [SerializeField] private IntVariableSO playerPolarityVar;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onScreenTransitionClosed;
        [SerializeField] private VoidEventChannelSO onScreenTransitionOpened;

        private Coroutine transitionCoroutine;
        private Vector3 defaultMaskWorldPosition;
        private Vector3 currentTransitionOriginWorldPosition;

        private void Awake()
        {
            if (maskSprite == null)
                return;

            defaultMaskWorldPosition = maskSprite.transform.position;
            currentTransitionOriginWorldPosition = defaultMaskWorldPosition;
        }

        private void Start()
        {
            if (maskSprite == null) return;

            var tex = CircleTextureGenerator.Create(128);
            maskSprite.sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
            maskSprite.color = TransitionColorHelper.WhitePolarityColor;
            maskSprite.transform.position = defaultMaskWorldPosition;
            maskSprite.transform.localScale = Vector3.zero;
        }

        private void OnDestroy()
        {
            if (maskSprite != null && maskSprite.sprite != null)
            {
                var tex = maskSprite.sprite.texture;
                Destroy(maskSprite.sprite);
                Destroy(tex);
            }
        }

        public void Close()
        {
            SetTransitionOriginToDefault();
            StartCloseTransition();
        }

        public void Close(Vector2 screenPosition)
        {
            SetTransitionOriginFromScreenPosition(screenPosition);
            StartCloseTransition();
        }

        public void Open()
        {
            StartOpenTransition();
        }

        public void Open(Vector2 screenPosition)
        {
            SetTransitionOriginFromScreenPosition(screenPosition);
            StartOpenTransition();
        }

        /// <summary>
        /// Instantly clears the transition mask without animation.
        /// Used after scene load to reveal the new scene immediately.
        /// </summary>
        public void ClearImmediate()
        {
            if (maskSprite == null) return;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;

            maskSprite.transform.localScale = Vector3.zero;
            maskSprite.transform.position = defaultMaskWorldPosition;
        }

        private void StartCloseTransition()
        {
            if (maskSprite == null) return;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(CloseCoroutine());
        }

        private void StartOpenTransition()
        {
            if (maskSprite == null) return;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(OpenCoroutine());
        }

        private IEnumerator CloseCoroutine()
        {
            // Set mask color to player's polarity color
            if (playerPolarityVar != null)
            {
                maskSprite.color = TransitionColorHelper.GetColor(playerPolarityVar.Value);
            }
            else
            {
                maskSprite.color = TransitionColorHelper.WhitePolarityColor;
            }

            maskSprite.transform.position = currentTransitionOriginWorldPosition;
            maskSprite.gameObject.SetActive(true);
            maskSprite.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = t * t; // EaseInQuad – decisive closing
                float scale = Mathf.Lerp(0f, maxScale, eased);
                maskSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            maskSprite.transform.localScale = new Vector3(maxScale, maxScale, 1f);
            transitionCoroutine = null;

            if (onScreenTransitionClosed != null)
                onScreenTransitionClosed.RaiseEvent();
        }

        private IEnumerator OpenCoroutine()
        {
            maskSprite.transform.position = currentTransitionOriginWorldPosition;
            maskSprite.gameObject.SetActive(true);
            maskSprite.transform.localScale = new Vector3(maxScale, maxScale, 1f);

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = 1f - (1f - t) * (1f - t); // EaseOutQuad – welcoming opening
                float scale = Mathf.Lerp(maxScale, 0f, eased);
                maskSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            maskSprite.transform.localScale = Vector3.zero;
            transitionCoroutine = null;

            if (onScreenTransitionOpened != null)
                onScreenTransitionOpened.RaiseEvent();
        }

        private void SetTransitionOriginToDefault()
        {
            currentTransitionOriginWorldPosition = defaultMaskWorldPosition;
        }

        private void SetTransitionOriginFromScreenPosition(Vector2 screenPosition)
        {
            Camera targetCamera = GetTransitionCamera();
            if (targetCamera == null)
            {
                SetTransitionOriginToDefault();
                return;
            }

            float distanceFromCamera = Mathf.Abs(defaultMaskWorldPosition.z - targetCamera.transform.position.z);
            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera));
            worldPosition.z = defaultMaskWorldPosition.z;
            currentTransitionOriginWorldPosition = worldPosition;
        }

        private Camera GetTransitionCamera()
        {
            if (transitionCamera != null)
                return transitionCamera;

            transitionCamera = Camera.main;

            if (transitionCamera == null)
                Debug.LogWarning($"[{GetType().Name}] transitionCamera not assigned on {gameObject.name}.", this);

            return transitionCamera;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (transitionCamera == null)
                Debug.LogWarning($"[{GetType().Name}] transitionCamera not assigned on {gameObject.name}.", this);
            if (maskSprite == null)
                Debug.LogWarning($"[{GetType().Name}] maskSprite not assigned on {gameObject.name}.", this);
            if (transitionDuration <= 0f)
                Debug.LogWarning($"[{GetType().Name}] transitionDuration should be greater than 0 on {gameObject.name}.", this);
            if (maxScale <= 0f)
                Debug.LogWarning($"[{GetType().Name}] maxScale should be greater than 0 on {gameObject.name}.", this);
            if (onScreenTransitionClosed == null)
                Debug.LogWarning($"[{GetType().Name}] onScreenTransitionClosed not assigned on {gameObject.name}.", this);
            if (onScreenTransitionOpened == null)
                Debug.LogWarning($"[{GetType().Name}] onScreenTransitionOpened not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null)
                Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
