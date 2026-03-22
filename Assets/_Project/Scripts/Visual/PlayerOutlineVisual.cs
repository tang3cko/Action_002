using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class PlayerOutlineVisual : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        [Header("Settings")]
        [SerializeField] private float outlineThickness = 0.3f;

        private SpriteRenderer outlineRenderer;
        private Texture2D outlineTexture;

        private void Awake()
        {
            var outlineGo = new GameObject("Outline");
            outlineGo.transform.SetParent(transform);
            outlineGo.transform.localPosition = new Vector3(0f, 0f, 0.01f);

            outlineRenderer = outlineGo.AddComponent<SpriteRenderer>();
            outlineTexture = CircleTextureGenerator.Create(64);
            outlineRenderer.sprite = Sprite.Create(
                outlineTexture,
                new Rect(0, 0, outlineTexture.width, outlineTexture.height),
                new Vector2(0.5f, 0.5f),
                outlineTexture.width
            );
            outlineRenderer.sortingOrder = -1;
            outlineRenderer.color = PolarityColors.GetBackground(0);
        }

        private void OnEnable()
        {
            if (onPolarityChanged == null) return;
            onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            if (onPolarityChanged == null) return;
            onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void LateUpdate()
        {
            if (outlineRenderer == null) return;
            outlineRenderer.transform.localScale = Vector3.one + Vector3.one * outlineThickness;
        }

        private void HandlePolarityChanged(int polarity)
        {
            if (outlineRenderer == null) return;
            outlineRenderer.color = PolarityColors.GetBackground(polarity);
        }

        private void OnDestroy()
        {
            if (outlineTexture != null)
                Destroy(outlineTexture);
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
