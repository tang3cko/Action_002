using UnityEngine;
using LitMotion;

namespace Action002.Visual
{
    public class PlayerCoreVisual : MonoBehaviour
    {
        private const float CoreScale = 0.3f;
        private const float PulseMin = 0.9f;
        private const float PulseMax = 1.1f;
        private const float PulseDuration = 0.2f;

        private SpriteRenderer _coreRenderer;
        private Texture2D _coreTexture;
        private Transform _coreTransform;
        private MotionHandle _pulseHandle;

        private void Awake()
        {
            var coreGo = new GameObject("Core");
            coreGo.transform.SetParent(transform, false);
            coreGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            coreGo.transform.localScale = Vector3.one * CoreScale;

            _coreTransform = coreGo.transform;

            _coreTexture = CircleTextureGenerator.Create(32);

            var sprite = Sprite.Create(
                _coreTexture,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0.5f),
                32f
            );

            _coreRenderer = coreGo.AddComponent<SpriteRenderer>();
            _coreRenderer.sprite = sprite;
            _coreRenderer.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            _coreRenderer.sortingOrder = 1;
        }

        private void OnEnable()
        {
            StartPulse();
        }

        private void OnDisable()
        {
            CancelPulse();
        }

        private void OnDestroy()
        {
            if (_coreTexture != null)
            {
                Destroy(_coreTexture);
            }
        }

        private void StartPulse()
        {
            if (_coreTransform == null)
            {
                return;
            }

            _pulseHandle = LMotion.Create(PulseMin, PulseMax, PulseDuration)
                .WithEase(Ease.InOutSine)
                .WithLoops(-1, LoopType.Yoyo)
                .Bind(scale => _coreTransform.localScale = Vector3.one * (CoreScale * scale));
        }

        private void CancelPulse()
        {
            if (_pulseHandle.IsActive())
            {
                _pulseHandle.Cancel();
            }
        }
    }
}
