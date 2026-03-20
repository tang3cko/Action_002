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
        [SerializeField] private float bulletSize = 0.4f;

        [Header("Outline")]
        [SerializeField] private Material whiteOutlineMaterial;
        [SerializeField] private Material blackOutlineMaterial;
        [SerializeField] private float outlineScale = 1.4f;

        [Header("Player Bullets")]
        [SerializeField] private float playerBulletScale = 0.7f;

        private const int BatchSize = 1023;
        private Matrix4x4[] whiteMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] blackMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] whiteOutlineMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] blackOutlineMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] playerWhiteMatrices = new Matrix4x4[BatchSize];
        private Matrix4x4[] playerBlackMatrices = new Matrix4x4[BatchSize];
        private Texture2D generatedTexture;

        private void Start()
        {
            generatedTexture = CircleTextureGenerator.Create(64);
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
            if (bulletSet == null || bulletSet.Count == 0) return;

            int whiteCount = 0;
            int blackCount = 0;
            int whiteOutlineCount = 0;
            int blackOutlineCount = 0;
            int playerWhiteCount = 0;
            int playerBlackCount = 0;
            float playerSize = bulletSize * playerBulletScale;
            var data = bulletSet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];

                if (state.Faction == 0)
                {
                    // Player bullet: smaller size, no outline
                    var matrix = Matrix4x4.TRS(
                        new Vector3(state.Position.x, state.Position.y, 0.05f),
                        Quaternion.identity,
                        Vector3.one * playerSize
                    );

                    if (state.Polarity == 0)
                    {
                        playerWhiteMatrices[playerWhiteCount++] = matrix;
                        if (playerWhiteCount == BatchSize)
                        {
                            if (quadMesh != null && whiteMaterial != null)
                                Graphics.DrawMeshInstanced(quadMesh, 0, whiteMaterial, playerWhiteMatrices, playerWhiteCount);
                            playerWhiteCount = 0;
                        }
                    }
                    else
                    {
                        playerBlackMatrices[playerBlackCount++] = matrix;
                        if (playerBlackCount == BatchSize)
                        {
                            if (quadMesh != null && blackMaterial != null)
                                Graphics.DrawMeshInstanced(quadMesh, 0, blackMaterial, playerBlackMatrices, playerBlackCount);
                            playerBlackCount = 0;
                        }
                    }
                }
                else
                {
                    // Enemy bullet: normal size with outline
                    var matrix = Matrix4x4.TRS(
                        new Vector3(state.Position.x, state.Position.y, 0.1f),
                        Quaternion.identity,
                        Vector3.one * bulletSize
                    );

                    var outlineMatrix = Matrix4x4.TRS(
                        new Vector3(state.Position.x, state.Position.y, 0.15f),
                        Quaternion.identity,
                        Vector3.one * (bulletSize * outlineScale)
                    );

                    if (state.Polarity == 0)
                    {
                        whiteMatrices[whiteCount++] = matrix;
                        whiteOutlineMatrices[whiteOutlineCount++] = outlineMatrix;

                        if (whiteOutlineCount == BatchSize)
                        {
                            if (quadMesh != null && whiteOutlineMaterial != null)
                                Graphics.DrawMeshInstanced(quadMesh, 0, whiteOutlineMaterial, whiteOutlineMatrices, whiteOutlineCount);
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
                        blackOutlineMatrices[blackOutlineCount++] = outlineMatrix;

                        if (blackOutlineCount == BatchSize)
                        {
                            if (quadMesh != null && blackOutlineMaterial != null)
                                Graphics.DrawMeshInstanced(quadMesh, 0, blackOutlineMaterial, blackOutlineMatrices, blackOutlineCount);
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
            }

            // Draw player bullets first (behind enemy bullets)
            if (playerWhiteCount > 0 && whiteMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, whiteMaterial, playerWhiteMatrices, playerWhiteCount);
            if (playerBlackCount > 0 && blackMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, blackMaterial, playerBlackMatrices, playerBlackCount);

            // Draw enemy outlines (behind enemy bodies)
            if (whiteOutlineCount > 0 && whiteOutlineMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, whiteOutlineMaterial, whiteOutlineMatrices, whiteOutlineCount);
            if (blackOutlineCount > 0 && blackOutlineMaterial != null && quadMesh != null)
                Graphics.DrawMeshInstanced(quadMesh, 0, blackOutlineMaterial, blackOutlineMatrices, blackOutlineCount);

            // Draw enemy bodies on top
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
