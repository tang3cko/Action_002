using UnityEngine;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Action002.Visual;

namespace Action002.Enemy.Rendering
{
    public class EnemyRenderer : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Rendering")]
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material baseMaterial;

        [Header("Visual Config")]
        [SerializeField] private EnemyVisualConfigSO visualConfig;

        [Header("Outline")]
        [SerializeField] private float outlineScale = 1.3f;

        private const int BatchSize = 1023;

        private Material bodyMaterial;
        private Material outlineMaterial;
        private Texture2D fallbackTexture;
        private MaterialPropertyBlock bodyBlock;
        private MaterialPropertyBlock outlineBlock;

        // Per EnemyTypeId × Polarity batch arrays
        private Matrix4x4[][] bodyBatches;
        private Matrix4x4[][] outlineBatches;
        private int[] bodyCounts;
        private int[] outlineCounts;
        private int typeCount;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        private void Start()
        {
            fallbackTexture = DiamondTextureGenerator.Create(64);

            if (baseMaterial != null)
            {
                bodyMaterial = Instantiate(baseMaterial);
                SetupMaterialAlphaClip(bodyMaterial, fallbackTexture);

                outlineMaterial = Instantiate(baseMaterial);
                SetupMaterialAlphaClip(outlineMaterial, fallbackTexture);
                outlineMaterial.SetFloat("_ZWrite", 0f);
            }

            bodyBlock = new MaterialPropertyBlock();
            outlineBlock = new MaterialPropertyBlock();

            var typeValues = System.Enum.GetValues(typeof(EnemyTypeId));
            int maxTypeIndex = 0;
            foreach (EnemyTypeId id in typeValues)
                if ((int)id > maxTypeIndex) maxTypeIndex = (int)id;
            typeCount = maxTypeIndex + 1;
            // 2 polarities per type
            int slotCount = typeCount * 2;
            bodyBatches = new Matrix4x4[slotCount][];
            outlineBatches = new Matrix4x4[slotCount][];
            bodyCounts = new int[slotCount];
            outlineCounts = new int[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                bodyBatches[i] = new Matrix4x4[BatchSize];
                outlineBatches[i] = new Matrix4x4[BatchSize];
            }
        }

        private void OnDestroy()
        {
            if (fallbackTexture != null) Destroy(fallbackTexture);
            if (bodyMaterial != null) Destroy(bodyMaterial);
            if (outlineMaterial != null) Destroy(outlineMaterial);
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
            if (bodyMaterial == null || outlineMaterial == null || quadMesh == null) return;

            int slotCount = typeCount * 2;
            for (int i = 0; i < slotCount; i++)
            {
                bodyCounts[i] = 0;
                outlineCounts[i] = 0;
            }

            var data = enemySet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];
                int typeIndex = (int)state.TypeId;
                int polarityBit = state.Polarity == 0 ? 0 : 1;
                int slot = typeIndex * 2 + polarityBit;

                float size = EnemyTypeTable.Get(state.TypeId).VisualScale;
                float outlineSizeValue = size * outlineScale;

                var bodyMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, 0.04f),
                    Quaternion.identity,
                    Vector3.one * size
                );
                var outMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, 0.06f),
                    Quaternion.identity,
                    Vector3.one * outlineSizeValue
                );

                bodyBatches[slot][bodyCounts[slot]++] = bodyMatrix;
                if (bodyCounts[slot] == BatchSize)
                {
                    FlushBody(slot);
                    bodyCounts[slot] = 0;
                }

                outlineBatches[slot][outlineCounts[slot]++] = outMatrix;
                if (outlineCounts[slot] == BatchSize)
                {
                    FlushOutline(slot);
                    outlineCounts[slot] = 0;
                }
            }

            // Draw outlines first (behind bodies)
            for (int slot = 0; slot < slotCount; slot++)
            {
                if (outlineCounts[slot] > 0)
                    FlushOutline(slot);
            }

            // Draw bodies on top
            for (int slot = 0; slot < slotCount; slot++)
            {
                if (bodyCounts[slot] > 0)
                    FlushBody(slot);
            }
        }

        private void FlushBody(int slot)
        {
            int typeIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForType((EnemyTypeId)typeIndex);
            bodyBlock.SetTexture(MainTexId, tex);
            bodyBlock.SetColor(ColorId, PolarityColors.GetForeground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, bodyMaterial, bodyBatches[slot], bodyCounts[slot], bodyBlock);
        }

        private void FlushOutline(int slot)
        {
            int typeIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForType((EnemyTypeId)typeIndex);
            outlineBlock.SetTexture(MainTexId, tex);
            outlineBlock.SetColor(ColorId, PolarityColors.GetBackground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, outlineMaterial, outlineBatches[slot], outlineCounts[slot], outlineBlock);
        }

        private Texture2D GetTextureForType(EnemyTypeId typeId)
        {
            if (visualConfig != null)
            {
                var tex = visualConfig.GetTexture(typeId);
                if (tex != null) return tex;
            }
            return fallbackTexture;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (quadMesh == null) Debug.LogWarning($"[{GetType().Name}] quadMesh not assigned on {gameObject.name}.", this);
            if (baseMaterial == null) Debug.LogWarning($"[{GetType().Name}] baseMaterial not assigned on {gameObject.name}.", this);
            if (visualConfig == null) Debug.LogWarning($"[{GetType().Name}] visualConfig not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
