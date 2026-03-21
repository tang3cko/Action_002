using System.Collections;
using UnityEngine;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class BackgroundPolarityEffect : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        [Header("References")]
        [SerializeField] private Vector2VariableSO playerPositionVar;

        private Camera mainCamera;

        [Header("Settings")]
        [SerializeField] private float transitionDuration = 0.4f;
        [SerializeField] private float maxScale = 60f;

        private static Color WhitePolarityBg => PolarityColors.WhiteBackground;
        private static Color BlackPolarityBg => PolarityColors.BlackBackground;

        private SpriteRenderer transitionSprite;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            mainCamera = Camera.main;

            if (mainCamera != null)
                mainCamera.backgroundColor = WhitePolarityBg;

            var go = new GameObject("PolarityTransitionCircle");
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.zero;
            transitionSprite = go.AddComponent<SpriteRenderer>();
            var tex = CircleTextureGenerator.Create(128);
            transitionSprite.sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
            transitionSprite.sortingOrder = -100;
            go.SetActive(false);
        }

        private void OnEnable()
        {
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void HandlePolarityChanged(int polarity)
        {
            Color targetColor = polarity == (int)Polarity.White ? WhitePolarityBg : BlackPolarityBg;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                if (mainCamera != null)
                    mainCamera.backgroundColor = transitionSprite.color;
                transitionSprite.gameObject.SetActive(false);
            }

            transitionCoroutine = StartCoroutine(TransitionCoroutine(targetColor));
        }

        private IEnumerator TransitionCoroutine(Color targetColor)
        {
            transitionSprite.color = targetColor;
            transitionSprite.gameObject.SetActive(true);

            if (playerPositionVar != null)
            {
                var pos = playerPositionVar.Value;
                transitionSprite.transform.position = new Vector3(pos.x, pos.y, 5f);
            }

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = 1f - (1f - t) * (1f - t); // EaseOutQuad
                float scale = eased * maxScale;
                transitionSprite.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            if (mainCamera != null)
                mainCamera.backgroundColor = targetColor;
            transitionSprite.gameObject.SetActive(false);
            transitionSprite.transform.localScale = Vector3.zero;
            transitionCoroutine = null;
        }

        private void OnDestroy()
        {
            if (transitionSprite != null && transitionSprite.sprite != null)
            {
                var tex = transitionSprite.sprite.texture;
                Destroy(transitionSprite.sprite);
                Destroy(tex);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onPolarityChanged == null)
                Debug.LogWarning($"[{GetType().Name}] onPolarityChanged not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null)
                Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
