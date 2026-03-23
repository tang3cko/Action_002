using UnityEngine;
using Action002.Accessory.SonicWave.Data;
using Action002.Core;
using Action002.Visual;

namespace Action002.Accessory.SonicWave.Rendering
{
    public class WaveRenderer : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private WaveStateSetSO waveSet;

        [Header("Rendering")]
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material waveMaterial;
        [SerializeField] private int sortingLayer = 0;
        [SerializeField] private float waveZ = -0.5f;

        private MaterialPropertyBlock propertyBlock;

        private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");
        private static readonly int RING_RADIUS_ID = Shader.PropertyToID("_RingRadius");
        private static readonly int RING_THICKNESS_ID = Shader.PropertyToID("_RingThickness");


        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            if (waveSet == null || waveSet.Count == 0) return;
            if (quadMesh == null || waveMaterial == null || gameConfig == null) return;

            float ringThickness = gameConfig.WaveRingThickness;
            var data = waveSet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var wave = data[i];
                float meshHalfSize = wave.MaxRadius + ringThickness * 0.5f;

                // normalized ring radius/thickness for the shader (0..1 across meshHalfSize)
                float normalizedRadius = wave.CurrentRadius / (meshHalfSize * 2f) * 2f;
                float normalizedThickness = ringThickness / (meshHalfSize * 2f) * 2f;

                propertyBlock.SetColor(BASE_COLOR_ID, PolarityColors.GetWaveRing(wave.Polarity));
                propertyBlock.SetFloat(RING_RADIUS_ID, normalizedRadius);
                propertyBlock.SetFloat(RING_THICKNESS_ID, normalizedThickness);

                // スケール: meshHalfSize * 2 で全体の大きさを決定
                float scale = meshHalfSize * 2f;

                var matrix = Matrix4x4.TRS(
                    new Vector3(wave.Origin.x, wave.Origin.y, waveZ),
                    Quaternion.identity,
                    Vector3.one * scale);

                Graphics.DrawMesh(quadMesh, matrix, waveMaterial, sortingLayer, null, 0, propertyBlock);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (waveSet == null) Debug.LogWarning($"[{GetType().Name}] waveSet not assigned on {gameObject.name}.", this);
            if (quadMesh == null) Debug.LogWarning($"[{GetType().Name}] quadMesh not assigned on {gameObject.name}.", this);
            if (waveMaterial == null) Debug.LogWarning($"[{GetType().Name}] waveMaterial not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
