using UnityEngine;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    /// <summary>
    /// Generates a fullscreen quad and drives the TitleBackground shader
    /// with polarity-aware yin-yang colour split.
    /// </summary>
    public class TitleBackgroundController : MonoBehaviour
    {
        // ── Fields ──────────────────────────────────────────

        [Header("Shader")]
        [SerializeField] private Shader waveShader;

        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private IntVariableSO polarityVar;

        [Header("Settings")]
        [SerializeField] private float speed = 0.04f;
        [SerializeField] private float scale = 3.0f;
        [SerializeField] private float distortion = 1.0f;
        [SerializeField] private float boundaryWidth = 0.15f;
        [SerializeField] private float tendrilStrength = 0.6f;

        [Header("Events")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        private Material titleMaterial;
        private GameObject quadObject;
        private float cachedAspect;
        private float cachedOrthoSize;
        private float cachedFov;
        private float cachedFarClip;
        private float cachedNearClip;
        private bool cachedOrthographic;

        // ── Shader Property IDs ─────────────────────────────

        private static readonly int DARK_COLOR_ID = Shader.PropertyToID("_DarkColor");
        private static readonly int LIGHT_COLOR_ID = Shader.PropertyToID("_LightColor");
        private static readonly int SPEED_ID = Shader.PropertyToID("_Speed");
        private static readonly int SCALE_ID = Shader.PropertyToID("_Scale");
        private static readonly int DISTORTION_ID = Shader.PropertyToID("_Distortion");
        private static readonly int BOUNDARY_WIDTH_ID = Shader.PropertyToID("_BoundaryWidth");
        private static readonly int TENDRIL_STRENGTH_ID = Shader.PropertyToID("_TendrilStrength");

        // ── Constants ───────────────────────────────────────

        private const float QUAD_DEPTH_MARGIN = 1f;

        // ── Unity Lifecycle ─────────────────────────────────

        private void Awake()
        {
            if (waveShader == null)
            {
                Debug.LogError($"[{GetType().Name}] waveShader is null on {gameObject.name}.", this);
                return;
            }

            if (targetCamera == null)
            {
                Debug.LogError($"[{GetType().Name}] targetCamera is null on {gameObject.name}.", this);
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

            if (titleMaterial != null)
                Destroy(titleMaterial);
        }

        // ── Private Methods ─────────────────────────────────

        private void CreateMaterial()
        {
            titleMaterial = new Material(waveShader);
            titleMaterial.SetFloat(SPEED_ID, speed);
            titleMaterial.SetFloat(SCALE_ID, scale);
            titleMaterial.SetFloat(DISTORTION_ID, distortion);
            titleMaterial.SetFloat(BOUNDARY_WIDTH_ID, boundaryWidth);
            titleMaterial.SetFloat(TENDRIL_STRENGTH_ID, tendrilStrength);
        }

        private void CreateQuad()
        {
            quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadObject.name = "TitleBackgroundQuad";

            var col = quadObject.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            quadObject.transform.SetParent(targetCamera.transform, false);
            float depth = CalculateQuadDepth();
            quadObject.transform.localPosition = new Vector3(0f, 0f, depth);

            UpdateQuadScale();

            var meshRenderer = quadObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = titleMaterial;
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
            if (titleMaterial == null) return;

            // White polarity: dark on left, light on right (default layout).
            // Black polarity: swap so light is on left, dark on right.
            Color darkColor;
            Color lightColor;

            if (polarity == (int)Polarity.White)
            {
                darkColor = PolarityColors.WhiteBackground;
                lightColor = PolarityColors.BlackBackground;
            }
            else
            {
                darkColor = PolarityColors.BlackBackground;
                lightColor = PolarityColors.WhiteBackground;
            }

            titleMaterial.SetColor(DARK_COLOR_ID, darkColor);
            titleMaterial.SetColor(LIGHT_COLOR_ID, lightColor);
        }

        // ── Editor Only ─────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (waveShader == null)
                Debug.LogWarning($"[{GetType().Name}] waveShader not assigned on {gameObject.name}.", this);
            if (targetCamera == null)
                Debug.LogWarning($"[{GetType().Name}] targetCamera not assigned on {gameObject.name}.", this);
            if (polarityVar == null)
                Debug.LogWarning($"[{GetType().Name}] polarityVar not assigned on {gameObject.name}.", this);
            if (onPolarityChanged == null)
                Debug.LogWarning($"[{GetType().Name}] onPolarityChanged not assigned on {gameObject.name}.", this);

            speed = Mathf.Max(0f, speed);
            scale = Mathf.Max(0.01f, scale);
            distortion = Mathf.Max(0f, distortion);
            boundaryWidth = Mathf.Max(0.01f, boundaryWidth);
            tendrilStrength = Mathf.Clamp(tendrilStrength, 0f, 2f);
        }
#endif
    }
}
