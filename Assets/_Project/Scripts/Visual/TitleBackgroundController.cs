using UnityEngine;
using Action002.Core;

namespace Action002.Visual
{
    public class TitleBackgroundController : MonoBehaviour
    {
        [Header("Shader")]
        [SerializeField] private Shader waveShader;

        [Header("Settings")]
        [SerializeField] private float speed = 0.04f;
        [SerializeField] private float scale = 3.0f;

        private Camera targetCamera;
        private Material titleMaterial;
        private GameObject quadObject;
        private float cachedAspect;
        private float cachedOrthoSize;
        private float cachedFov;
        private float cachedFarClip;
        private float cachedNearClip;
        private bool cachedOrthographic;

        private static readonly int DARK_COLOR_ID = Shader.PropertyToID("_DarkColor");
        private static readonly int LIGHT_COLOR_ID = Shader.PropertyToID("_LightColor");
        private static readonly int SPEED_ID = Shader.PropertyToID("_Speed");
        private static readonly int SCALE_ID = Shader.PropertyToID("_Scale");

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
            ApplyColors();
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

        private void OnDestroy()
        {
            if (quadObject != null)
                Destroy(quadObject);

            if (titleMaterial != null)
                Destroy(titleMaterial);
        }

        private void CreateMaterial()
        {
            titleMaterial = new Material(waveShader);
            titleMaterial.SetFloat(SPEED_ID, speed);
            titleMaterial.SetFloat(SCALE_ID, scale);
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

        private void ApplyColors()
        {
            if (titleMaterial == null) return;

            titleMaterial.SetColor(DARK_COLOR_ID, PolarityColors.WhiteBackground);
            titleMaterial.SetColor(LIGHT_COLOR_ID, PolarityColors.BlackBackground);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (waveShader == null)
                Debug.LogWarning($"[{GetType().Name}] waveShader not assigned on {gameObject.name}.", this);

            speed = Mathf.Max(0f, speed);
            scale = Mathf.Max(0.01f, scale);
        }
#endif
    }
}
