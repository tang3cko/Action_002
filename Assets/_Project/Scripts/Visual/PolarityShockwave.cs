using UnityEngine;
using LitMotion;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class PolarityShockwave : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        [Header("Dependencies")]
        [SerializeField] private Vector2VariableSO playerPositionVar;

        [Header("Settings")]
        [SerializeField] private float maxScale = 8f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float startAlpha = 0.5f;

        private SpriteRenderer shockwaveRenderer;
        private Texture2D ringTexture;
        private MotionHandle motionHandle;

        private void Awake()
        {
            var go = new GameObject("Shockwave");
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.zero;

            shockwaveRenderer = go.AddComponent<SpriteRenderer>();
            ringTexture = CreateRingTexture(64);
            shockwaveRenderer.sprite = Sprite.Create(
                ringTexture,
                new Rect(0, 0, ringTexture.width, ringTexture.height),
                new Vector2(0.5f, 0.5f),
                ringTexture.width);
            shockwaveRenderer.sortingOrder = 10;

            var c = shockwaveRenderer.color;
            c.a = 0f;
            shockwaveRenderer.color = c;
        }

        private void OnEnable()
        {
            if (onPolarityChanged == null)
                return;

            onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            CancelAnimation();

            if (onPolarityChanged == null)
                return;

            onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void OnDestroy()
        {
            if (ringTexture != null)
                Destroy(ringTexture);
        }

        private void HandlePolarityChanged(int polarity)
        {
            CancelAnimation();

            if (playerPositionVar != null)
            {
                var pos = playerPositionVar.Value;
                shockwaveRenderer.transform.position = new Vector3(pos.x, pos.y, 0f);
            }

            Color baseColor = PolarityColors.GetForeground(polarity);
            baseColor.a = startAlpha;
            shockwaveRenderer.color = baseColor;
            shockwaveRenderer.transform.localScale = Vector3.zero;

            motionHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.OutQuad)
                .Bind(t =>
                {
                    float scale = t * maxScale;
                    shockwaveRenderer.transform.localScale = new Vector3(scale, scale, 1f);

                    Color c = shockwaveRenderer.color;
                    c.a = Mathf.Lerp(startAlpha, 0f, t);
                    shockwaveRenderer.color = c;
                });
        }

        private void CancelAnimation()
        {
            if (motionHandle.IsActive())
            {
                motionHandle.Cancel();

                Color c = shockwaveRenderer.color;
                c.a = 0f;
                shockwaveRenderer.color = c;
            }
        }

        private static Texture2D CreateRingTexture(int resolution)
        {
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            int totalPixels = resolution * resolution;
            var pixels = new Color32[totalPixels];
            float center = resolution * 0.5f;
            float innerRadius = resolution * 0.35f;
            float outerRadius = resolution * 0.5f;
            float edgeWidth = 1.5f;

            for (int i = 0; i < totalPixels; i++)
            {
                float dx = (i % resolution) + 0.5f - center;
                float dy = (i / resolution) + 0.5f - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                byte alpha;
                if (dist < innerRadius - edgeWidth || dist > outerRadius + edgeWidth)
                {
                    alpha = 0;
                }
                else if (dist >= innerRadius && dist <= outerRadius)
                {
                    alpha = 255;
                }
                else if (dist < innerRadius)
                {
                    alpha = (byte)(255f * (dist - (innerRadius - edgeWidth)) / edgeWidth);
                }
                else
                {
                    alpha = (byte)(255f * (1f - (dist - outerRadius) / edgeWidth));
                }

                pixels[i] = new Color32(255, 255, 255, alpha);
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
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
