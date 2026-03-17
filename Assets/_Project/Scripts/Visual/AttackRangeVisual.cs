using System.Collections;
using UnityEngine;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class AttackRangeVisual : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Events")]
        [SerializeField] private IntEventChannelSO onEnemyKilled;
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        [Header("Settings")]
        [SerializeField] private float baseAlpha = 0.08f;
        [SerializeField] private float flashAlpha = 0.3f;
        [SerializeField] private float flashDuration = 0.1f;

        private SpriteRenderer _rangeRenderer;
        private Coroutine _flashCoroutine;
        private Color _currentColor;

        private static readonly Color WhitePolarityRangeColor = new Color(0.878f, 0.878f, 1f);
        private static readonly Color BlackPolarityRangeColor = new Color(0.15f, 0.15f, 0.25f);

        private void Awake()
        {
            var go = new GameObject("AttackRangeIndicator");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _rangeRenderer = go.AddComponent<SpriteRenderer>();

            var tex = CircleTextureGenerator.Create(64);
            _rangeRenderer.sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width * 0.5f);
            _rangeRenderer.sortingOrder = -1;

            _currentColor = WhitePolarityRangeColor;
            UpdateVisual();
        }

        private void Start()
        {
            if (gameConfig != null)
            {
                float diameter = gameConfig.AttackRange * 2f;
                _rangeRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }

        private void OnEnable()
        {
            if (onEnemyKilled != null)
                onEnemyKilled.OnEventRaised += HandleEnemyKilled;
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            if (onEnemyKilled != null)
                onEnemyKilled.OnEventRaised -= HandleEnemyKilled;
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void HandleEnemyKilled(int enemyPolarity)
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private void HandlePolarityChanged(int polarity)
        {
            _currentColor = polarity == 0 ? WhitePolarityRangeColor : BlackPolarityRangeColor;
            UpdateVisual();
        }

        private IEnumerator FlashCoroutine()
        {
            SetAlpha(flashAlpha);
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / flashDuration);
                SetAlpha(Mathf.Lerp(flashAlpha, baseAlpha, t));
                yield return null;
            }
            SetAlpha(baseAlpha);
            _flashCoroutine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (_rangeRenderer == null) return;
            var c = _currentColor;
            c.a = alpha;
            _rangeRenderer.color = c;
        }

        private void UpdateVisual()
        {
            SetAlpha(baseAlpha);
        }

        private void OnDestroy()
        {
            if (_rangeRenderer != null && _rangeRenderer.sprite != null)
            {
                var tex = _rangeRenderer.sprite.texture;
                Destroy(_rangeRenderer.sprite);
                Destroy(tex);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null)
                Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
