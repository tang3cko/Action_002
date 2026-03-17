using UnityEngine;
using Action002.Bullet.Data;
using Action002.Visual;
using Tang3cko.ReactiveSO;

namespace Action002.Bullet.Rendering
{
    public class BulletRenderer : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("Rendering")]
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;
        [SerializeField] private float bulletSize = 0.15f;

        [Header("Outline")]
        [SerializeField] private Material whiteOutlineMaterial;
        [SerializeField] private Material blackOutlineMaterial;
        [SerializeField] private float outlineScale = 1.4f;

        private const int BatchSize = 1023;
        private Matrix4x4[] whiteMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] blackMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] _whiteOutlineMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] _blackOutlineMatrices = new Matrix4x4[BatchSize];
        private Texture2D _generatedTexture;

        private void Start()
        {
            _generatedTexture = CircleTextureGenerator.Create(64);
            if (whiteMaterial != null)
            {
                whiteMaterial = Instantiate(whiteMaterial);
                SetupMaterialAlphaClip(whiteMaterial, _generatedTexture);
            }
            if (blackMaterial != null)
            {
                blackMaterial = Instantiate(blackMaterial);
                SetupMaterialAlphaClip(blackMaterial, _generatedTexture);
            }
            if (whiteOutlineMaterial != null)
            {
                whiteOutlineMaterial = Instantiate(whiteOutlineMaterial);
                SetupMaterialAlphaClip(whiteOutlineMaterial, _generatedTexture);
                whiteOutlineMaterial.SetFloat("_ZWrite", 0f);
            }
            if (blackOutlineMaterial != null)
            {
                blackOutlineMaterial = Instantiate(blackOutlineMaterial);
                SetupMaterialAlphaClip(blackOutlineMaterial, _generatedTexture);
                blackOutlineMaterial.SetFloat("_ZWrite", 0f);
            }
        }

        private void OnDestroy()
        {
            if (_generatedTexture != null) Destroy(_generatedTexture);
            if (whiteMaterial != null) Destroy(whiteMaterial);
            if (blackMaterial != null) Destroy(blackMaterial);
            if (whiteOutlineMaterial != null) Destroy(whiteOutlineMaterial);
            if (blackOutlineMaterial != null) Destroy(blackOutlineMaterial);
        }

        private static void SetupMaterialAlphaClip(Material mat, Texture2D tex)
        {
            if (mat == null) return;
            mat.mainTexture = tex;
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.1f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.renderQueue = 2450;
        }

        private void LateUpdate()
        {
            if (bulletSet == null || bulletSet.Count == 0) return;

            int whiteCount = 0;
            int blackCount = 0;
            int whiteOutlineCount = 0;
            int blackOutlineCount = 0;
            var data = bulletSet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];

                // Body matrix
                var matrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, 0.1f),
                    Quaternion.identity,
                    Vector3.one * bulletSize
                );

                // Outline matrix (behind body, slightly larger)
                var outlineMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, 0.15f),
                    Quaternion.identity,
                    Vector3.one * (bulletSize * outlineScale)
                );

                if (state.Polarity == 0)
                {
                    whiteMatrices[whiteCount++] = matrix;
                    _whiteOutlineMatrices[whiteOutlineCount++] = outlineMatrix;

                    if (whiteOutlineCount == BatchSize)
                    {
                        if (quadMesh != null && whiteOutlineMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, whiteOutlineMaterial, _whiteOutlineMatrices, whiteOutlineCount);
                        whiteOutlineCount = 0;
                    }
                    if (whiteCount == BatchSize)
                    {
                        if (quadMesh != null && whiteMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, whiteMaterial, whiteMatrices, whiteCount);
                        whiteCount = 0;
                    }
                }
                else
                {
                    blackMatrices[blackCount++] = matrix;
                    _blackOutlineMatrices[blackOutlineCount++] = outlineMatrix;

                    if (blackOutlineCount == BatchSize)
                    {
                        if (quadMesh != null && blackOutlineMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, blackOutlineMaterial, _blackOutlineMatrices, blackOutlineCount);
                        blackOutlineCount = 0;
                    }
                    if (blackCount == BatchSize)
                    {
                        if (quadMesh != null && blackMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, blackMaterial, blackMatrices, blackCount);
                        blackCount = 0;
                    }
                }
            }

            // Draw outlines first (behind bodies)
            if (whiteOutlineCount > 0 && whiteOutlineMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, whiteOutlineMaterial, _whiteOutlineMatrices, whiteOutlineCount);
            if (blackOutlineCount > 0 && blackOutlineMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, blackOutlineMaterial, _blackOutlineMatrices, blackOutlineCount);

            // Draw bodies on top
            if (whiteCount > 0 && whiteMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, whiteMaterial, whiteMatrices, whiteCount);
            if (blackCount > 0 && blackMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, blackMaterial, blackMatrices, blackCount);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (whiteOutlineMaterial == null) Debug.LogWarning($"[{GetType().Name}] whiteOutlineMaterial not assigned on {gameObject.name}.", this);
            if (blackOutlineMaterial == null) Debug.LogWarning($"[{GetType().Name}] blackOutlineMaterial not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
