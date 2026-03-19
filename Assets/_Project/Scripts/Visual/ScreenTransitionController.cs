using System.Collections;
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class ScreenTransitionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer maskSprite;

        [Header("Settings")]
        [SerializeField] private float transitionDuration = 0.4f;
        [SerializeField] private float maxScale = 60f;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onScreenTransitionClosed;
        [SerializeField] private VoidEventChannelSO onScreenTransitionOpened;

        private Coroutine transitionCoroutine;

        private void Start()
        {
            if (maskSprite == null) return;

            var tex = CircleTextureGenerator.Create(128);
            maskSprite.sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
            maskSprite.color = Color.black;
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
            if (maskSprite == null) return;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(CloseCoroutine());
        }

        public void Open()
        {
            if (maskSprite == null) return;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(OpenCoroutine());
        }

        private IEnumerator CloseCoroutine()
        {
            maskSprite.color = Color.black;
            maskSprite.gameObject.SetActive(true);
            maskSprite.transform.localScale = new Vector3(maxScale, maxScale, 1f);

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = 1f - (1f - t) * (1f - t); // EaseOutQuad
                float scale = Mathf.Lerp(maxScale, 0f, eased);
                maskSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            maskSprite.transform.localScale = Vector3.zero;
            transitionCoroutine = null;

            if (onScreenTransitionClosed != null)
                onScreenTransitionClosed.RaiseEvent();
        }

        private IEnumerator OpenCoroutine()
        {
            maskSprite.gameObject.SetActive(true);
            maskSprite.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = 1f - (1f - t) * (1f - t); // EaseOutQuad
                float scale = Mathf.Lerp(0f, maxScale, eased);
                maskSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            maskSprite.transform.localScale = new Vector3(maxScale, maxScale, 1f);
            transitionCoroutine = null;

            if (onScreenTransitionOpened != null)
                onScreenTransitionOpened.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (maskSprite == null)
                Debug.LogWarning($"[{GetType().Name}] maskSprite not assigned on {gameObject.name}.", this);
            if (onScreenTransitionClosed == null)
                Debug.LogWarning($"[{GetType().Name}] onScreenTransitionClosed not assigned on {gameObject.name}.", this);
            if (onScreenTransitionOpened == null)
                Debug.LogWarning($"[{GetType().Name}] onScreenTransitionOpened not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
