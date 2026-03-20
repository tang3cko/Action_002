using UnityEngine;
using Unity.Mathematics;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Action002.Visual;
using Tang3cko.ReactiveSO;

namespace Action002.Enemy.Rendering
{
    public class EnemyRenderer : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Rendering")]
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;

        [Header("Outline")]
        [SerializeField] private Material whiteOutlineMaterial;
        [SerializeField] private Material blackOutlineMaterial;
        [SerializeField] private float outlineScale = 1.3f;

        private const int BatchSize = 1023;
        private Matrix4x4[] whiteMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] blackMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] whiteOutlineMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] blackOutlineMatrices = new Matrix4x4[BatchSize];
        private Texture2D generatedTexture;

        private void Start()
        {
            generatedTexture = DiamondTextureGenerator.Create(64);
            if (whiteMaterial != null)
            {
                whiteMaterial = Instantiate(whiteMaterial);
                SetupMaterialAlphaClip(whiteMaterial, generatedTexture);
            }
            if (blackMaterial != null)
            {
                blackMaterial = Instantiate(blackMaterial);
                SetupMaterialAlphaClip(blackMaterial, generatedTexture);
            }
            if (whiteOutlineMaterial != null)
            {
                whiteOutlineMaterial = Instantiate(whiteOutlineMaterial);
                SetupMaterialAlphaClip(whiteOutlineMaterial, generatedTexture);
                whiteOutlineMaterial.SetFloat("_ZWrite", 0f);
            }
            if (blackOutlineMaterial != null)
            {
                blackOutlineMaterial = Instantiate(blackOutlineMaterial);
                SetupMaterialAlphaClip(blackOutlineMaterial, generatedTexture);
                blackOutlineMaterial.SetFloat("_ZWrite", 0f);
            }
        }

        private void OnDestroy()
        {
            if (generatedTexture != null) Destroy(generatedTexture);
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
            if (enemySet == null || enemySet.Count == 0) return;

            int whiteCount = 0;
            int blackCount = 0;
            int whiteOutlineCount = 0;
            int blackOutlineCount = 0;
            var data = enemySet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];
                float size = EnemyTypeTable.Get(state.TypeId).VisualScale;
                float outlineSizeValue = size * outlineScale;
                var bodyMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, 0f),
                    Quaternion.identity,
                    Vector3.one * size
                );
                var outlineMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, 0.05f),
                    Quaternion.identity,
                    Vector3.one * outlineSizeValue
                );

                if (state.Polarity == 0) // White
                {
                    whiteMatrices[whiteCount++] = bodyMatrix;
                    if (whiteCount == BatchSize)
                    {
                        if (quadMesh != null && whiteMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, whiteMaterial, whiteMatrices, whiteCount);
                        whiteCount = 0;
                    }

                    whiteOutlineMatrices[whiteOutlineCount++] = outlineMatrix;
                    if (whiteOutlineCount == BatchSize)
                    {
                        if (quadMesh != null && whiteOutlineMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, whiteOutlineMaterial, whiteOutlineMatrices, whiteOutlineCount);
                        whiteOutlineCount = 0;
                    }
                }
                else // Black
                {
                    blackMatrices[blackCount++] = bodyMatrix;
                    if (blackCount == BatchSize)
                    {
                        if (quadMesh != null && blackMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, blackMaterial, blackMatrices, blackCount);
                        blackCount = 0;
                    }

                    blackOutlineMatrices[blackOutlineCount++] = outlineMatrix;
                    if (blackOutlineCount == BatchSize)
                    {
                        if (quadMesh != null && blackOutlineMaterial != null)
                            Graphics.DrawMeshInstanced(quadMesh, 0, blackOutlineMaterial, blackOutlineMatrices, blackOutlineCount);
                        blackOutlineCount = 0;
                    }
                }
            }

            // Draw outlines BEFORE bodies (outlines are behind at z=0.05)
            if (whiteOutlineCount > 0 && whiteOutlineMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, whiteOutlineMaterial, whiteOutlineMatrices, whiteOutlineCount);
            if (blackOutlineCount > 0 && blackOutlineMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, blackOutlineMaterial, blackOutlineMatrices, blackOutlineCount);

            // Draw bodies on top
            if (whiteCount > 0 && whiteMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, whiteMaterial, whiteMatrices, whiteCount);
            if (blackCount > 0 && blackMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, blackMaterial, blackMatrices, blackCount);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (quadMesh == null) Debug.LogWarning($"[{GetType().Name}] quadMesh not assigned on {gameObject.name}.", this);
            if (whiteOutlineMaterial == null) Debug.LogWarning($"[{GetType().Name}] whiteOutlineMaterial not assigned on {gameObject.name}.", this);
            if (blackOutlineMaterial == null) Debug.LogWarning($"[{GetType().Name}] blackOutlineMaterial not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
