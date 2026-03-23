using UnityEngine;
using LitMotion;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageScreenFlash : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float flashAlpha = 0.3f;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private int sortingOrder = 999;
        [SerializeField] private float overlayDistance = -1f;
        [SerializeField] private Vector3 overlayScale = new Vector3(100f, 100f, 1f);

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;

        private Camera mainCamera;
        private SpriteRenderer overlayRenderer;
        private Texture2D overlayTexture;
        private Sprite overlaySprite;
        private MotionHandle flashHandle;

        private void Awake()
        {
            mainCamera = Camera.main;
            overlayRenderer = GetComponent<SpriteRenderer>();
            overlayTexture = CreateOverlayTexture();
            overlaySprite = CreateOverlaySprite(overlayTexture);

            overlayRenderer.sprite = overlaySprite;
            overlayRenderer.sortingOrder = sortingOrder;
            SetOverlayAlpha(0f);

            transform.localScale = overlayScale;
            PositionOverlay();
        }

        private void OnEnable()
        {
            if (onPlayerDamaged == null)
                return;

            onPlayerDamaged.OnEventRaised += HandlePlayerDamaged;
        }

        private void OnDisable()
        {
            CancelFlash();
            SetOverlayAlpha(0f);

            if (onPlayerDamaged == null)
                return;

            onPlayerDamaged.OnEventRaised -= HandlePlayerDamaged;
        }

        private void OnDestroy()
        {
            if (overlayTexture != null)
            {
                Destroy(overlayTexture);
            }

            if (overlaySprite != null)
            {
                Destroy(overlaySprite);
            }
        }

        private void HandlePlayerDamaged()
        {
            if (overlayRenderer == null)
            {
                return;
            }

            CancelFlash();
            PositionOverlay();
            SetOverlayAlpha(flashAlpha);

            flashHandle = LMotion.Create(flashAlpha, 0f, flashDuration)
                .WithEase(Ease.OutQuad)
                .Bind(SetOverlayAlpha);
        }

        private void CancelFlash()
        {
            if (flashHandle.IsActive())
            {
                flashHandle.Cancel();
            }
        }

        private void PositionOverlay()
        {
            Camera cameraToUse = mainCamera;
            if (cameraToUse == null)
            {
                return;
            }

            Vector3 cameraPosition = cameraToUse.transform.position;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, overlayDistance);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (overlayRenderer == null)
            {
                return;
            }

            Color color = overlayRenderer.color;
            color.a = alpha;
            overlayRenderer.color = color;
        }

        private static Texture2D CreateOverlayTexture()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Sprite CreateOverlaySprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onPlayerDamaged == null)
            {
                Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
            }
        }
#endif
    }
}
