using UnityEngine;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class BackgroundWaveController : MonoBehaviour
    {
        [Header("Shader")]
        [SerializeField] private Shader waveShader;

        [Header("References")]
        [SerializeField] private IntVariableSO polarityVar;

        [Header("Settings")]
        [SerializeField] private float waveIntensity = 2.0f;
        [SerializeField] private float speed = 0.03f;
        [SerializeField] private float scale = 3.0f;

        [Header("Events")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        private Camera targetCamera;
        private Material waveMaterial;
        private GameObject quadObject;
        private float cachedAspect;
        private float cachedOrthoSize;
        private float cachedFov;
        private float cachedFarClip;
        private float cachedNearClip;
        private bool cachedOrthographic;

        private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");
        private static readonly int WAVE_COLOR_ID = Shader.PropertyToID("_WaveColor");
        private static readonly int WAVE_INTENSITY_ID = Shader.PropertyToID("_WaveIntensity");
        private static readonly int SPEED_ID = Shader.PropertyToID("_Speed");
        private static readonly int SCALE_ID = Shader.PropertyToID("_Scale");

        private const float WAVE_COLOR_OFFSET = 0.5f;
        private const float QUAD_DEPTH_MARGIN = 1f;

        private void Awake()
        {
            if (waveShader == null)
            {
                Debug.LogError($"[{GetType().Name}] waveShader is null on {gameObject.name}.", this);
                return;
            }

            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError($"[{GetType().Name}] Main Camera not found.", this);
                return;
            }

            CreateMaterial();
            CreateQuad();

            int initialPolarity = polarityVar != null ? polarityVar.Value : (int)Polarity.White;
            ApplyPolarityColors(initialPolarity);
        }

        private void OnEnable()
        {
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void LateUpdate()
        {
            if (targetCamera == null || quadObject == null) return;

            bool hasChanged = cachedOrthographic != targetCamera.orthographic
                || !Mathf.Approximately(cachedAspect, targetCamera.aspect)
                || !Mathf.Approximately(cachedFarClip, targetCamera.farClipPlane)
                || !Mathf.Approximately(cachedNearClip, targetCamera.nearClipPlane);

            if (targetCamera.orthographic)
            {
                hasChanged = hasChanged
                    || !Mathf.Approximately(cachedOrthoSize, targetCamera.orthographicSize);
            }
            else
            {
                hasChanged = hasChanged
                    || !Mathf.Approximately(cachedFov, targetCamera.fieldOfView);
            }

            if (!hasChanged) return;

            float depth = CalculateQuadDepth();
            quadObject.transform.localPosition = new Vector3(0f, 0f, depth);
            UpdateQuadScale();
        }

        private void OnDisable()
        {
            if (onPolarityChanged != null)
                onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void OnDestroy()
        {
            if (quadObject != null)
                Destroy(quadObject);

            if (waveMaterial != null)
                Destroy(waveMaterial);
        }

        private void CreateMaterial()
        {
            waveMaterial = new Material(waveShader);
            waveMaterial.SetFloat(WAVE_INTENSITY_ID, waveIntensity);
            waveMaterial.SetFloat(SPEED_ID, speed);
            waveMaterial.SetFloat(SCALE_ID, scale);
        }

        private void CreateQuad()
        {
            quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadObject.name = "BackgroundWaveQuad";

            var col = quadObject.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            quadObject.transform.SetParent(targetCamera.transform, false);
            float depth = CalculateQuadDepth();
            quadObject.transform.localPosition = new Vector3(0f, 0f, depth);

            UpdateQuadScale();

            var meshRenderer = quadObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = waveMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        private void UpdateQuadScale()
        {
            if (quadObject == null) return;

            cachedAspect = targetCamera.aspect;
            cachedOrthoSize = targetCamera.orthographicSize;
            cachedFov = targetCamera.fieldOfView;
            cachedFarClip = targetCamera.farClipPlane;
            cachedNearClip = targetCamera.nearClipPlane;
            cachedOrthographic = targetCamera.orthographic;

            float distance = CalculateQuadDepth();

            float height;
            if (targetCamera.orthographic)
            {
                height = cachedOrthoSize * 2f;
            }
            else
            {
                height = 2f * distance * Mathf.Tan(cachedFov * 0.5f * Mathf.Deg2Rad);
            }

            float width = height * cachedAspect;
            quadObject.transform.localScale = new Vector3(width, height, 1f);
        }

        private float CalculateQuadDepth()
        {
            float near = targetCamera.nearClipPlane;
            float far = targetCamera.farClipPlane;
            float range = far - near;

            float margin = Mathf.Min(QUAD_DEPTH_MARGIN, range * 0.1f);
            return far - Mathf.Max(margin, Mathf.Epsilon);
        }

        private void HandlePolarityChanged(int polarity)
        {
            ApplyPolarityColors(polarity);
        }

        private void ApplyPolarityColors(int polarity)
        {
            if (waveMaterial == null) return;

            Color baseColor = PolarityColors.GetBackground(polarity);
            Color waveColor = CalculateWaveColor(baseColor, polarity);

            waveMaterial.SetColor(BASE_COLOR_ID, baseColor);
            waveMaterial.SetColor(WAVE_COLOR_ID, waveColor);
        }

        private static Color CalculateWaveColor(Color baseColor, int polarity)
        {
            float sign = polarity == (int)Polarity.White ? 1f : -1f;

            return new Color(
                Mathf.Clamp01(baseColor.r + sign * WAVE_COLOR_OFFSET),
                Mathf.Clamp01(baseColor.g + sign * WAVE_COLOR_OFFSET),
                Mathf.Clamp01(baseColor.b + sign * WAVE_COLOR_OFFSET),
                1f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (waveShader == null)
                Debug.LogWarning($"[{GetType().Name}] waveShader not assigned on {gameObject.name}.", this);
            if (polarityVar == null)
                Debug.LogWarning($"[{GetType().Name}] polarityVar not assigned on {gameObject.name}.", this);
            if (onPolarityChanged == null)
                Debug.LogWarning($"[{GetType().Name}] onPolarityChanged not assigned on {gameObject.name}.", this);

            waveIntensity = Mathf.Max(0f, waveIntensity);
            speed = Mathf.Max(0f, speed);
            scale = Mathf.Max(0.01f, scale);
        }
#endif
    }
}
