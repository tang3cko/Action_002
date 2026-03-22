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
        private const int BATCH_SIZE = 1023;
        private const float OUTLINE_Z = 0.06f;
        private const float BODY_Z = 0.04f;

        private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_BaseMap");
        private static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");

        [Header("Dependencies")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private EnemyVisualConfigSO visualConfig;

        [Header("Settings")]
        [SerializeField] private float outlineThickness = 0.24f;

        private Material bodyMaterial;
        private Material outlineMaterial;
        private Texture2D fallbackTexture;
        private MaterialPropertyBlock bodyBlock;
        private MaterialPropertyBlock outlineBlock;

        // Per EnemyTypeId x Polarity batch arrays
        private Matrix4x4[][] bodyBatches;
        private Matrix4x4[][] outlineBatches;
        private int[] bodyCounts;
        private int[] outlineCounts;
        private int slotCount;
        private int typeCount;

        private void Awake()
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

            typeCount = GetTypeCount();
            slotCount = GetSlotCount();
            bodyBatches = new Matrix4x4[slotCount][];
            outlineBatches = new Matrix4x4[slotCount][];
            bodyCounts = new int[slotCount];
            outlineCounts = new int[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                bodyBatches[i] = new Matrix4x4[BATCH_SIZE];
                outlineBatches[i] = new Matrix4x4[BATCH_SIZE];
            }
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

            ResetBatchCounts();

            var data = enemySet.Data;
            float2 playerPos = GetPlayerPosition();
            float time = Time.time;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];
                int slot = GetSlot(state.TypeId, state.Polarity);

                float size = EnemyTypeTable.Get(state.TypeId).VisualScale;
                float outlineSizeValue = size + outlineThickness;

                float spawnElapsed = time - state.SpawnTime;
                if (!EnemySpawnCalculator.IsComplete(spawnElapsed))
                {
                    size = EnemySpawnCalculator.CalculateScale(spawnElapsed, size);
                    outlineSizeValue = EnemySpawnCalculator.CalculateScale(spawnElapsed, outlineSizeValue);
                }

                float angle = EnemyRotationCalculator.CalculateAngle(state.TypeId, state.Position, playerPos, time);
                var rotation = Quaternion.Euler(0f, 0f, angle);

                var bodyMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, BODY_Z),
                    rotation,
                    Vector3.one * size
                );
                var outMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, OUTLINE_Z),
                    rotation,
                    Vector3.one * outlineSizeValue
                );

                bodyBatches[slot][bodyCounts[slot]++] = bodyMatrix;
                if (bodyCounts[slot] == BATCH_SIZE)
                {
                    FlushBody(slot);
                    bodyCounts[slot] = 0;
                }

                outlineBatches[slot][outlineCounts[slot]++] = outMatrix;
                if (outlineCounts[slot] == BATCH_SIZE)
                {
                    FlushOutline(slot);
                    outlineCounts[slot] = 0;
                }
            }

            FlushRemainingBatches();
        }

        private void OnDestroy()
        {
            if (fallbackTexture != null) Destroy(fallbackTexture);
            if (bodyMaterial != null) Destroy(bodyMaterial);
            if (outlineMaterial != null) Destroy(outlineMaterial);
        }

        private void FlushBody(int slot)
        {
            int typeIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForType((EnemyTypeId)typeIndex);
            bodyBlock.Clear();
            bodyBlock.SetTexture(MAIN_TEX_ID, tex);
            bodyBlock.SetColor(COLOR_ID, PolarityColors.GetForeground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, bodyMaterial, bodyBatches[slot], bodyCounts[slot], bodyBlock);
        }

        private void FlushOutline(int slot)
        {
            int typeIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForType((EnemyTypeId)typeIndex);
            outlineBlock.Clear();
            outlineBlock.SetTexture(MAIN_TEX_ID, tex);
            outlineBlock.SetColor(COLOR_ID, PolarityColors.GetBackground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, outlineMaterial, outlineBatches[slot], outlineCounts[slot], outlineBlock);
        }

        private void ResetBatchCounts()
        {
            for (int i = 0; i < slotCount; i++)
            {
                bodyCounts[i] = 0;
                outlineCounts[i] = 0;
            }
        }

        private void FlushRemainingBatches()
        {
            for (int slot = 0; slot < slotCount; slot++)
            {
                if (outlineCounts[slot] > 0)
                {
                    FlushOutline(slot);
                }
            }

            for (int slot = 0; slot < slotCount; slot++)
            {
                if (bodyCounts[slot] > 0)
                {
                    FlushBody(slot);
                }
            }
        }

        private float2 GetPlayerPosition()
        {
            if (playerPositionVar == null)
            {
                return float2.zero;
            }

            Vector2 playerPosition = playerPositionVar.Value;
            return new float2(playerPosition.x, playerPosition.y);
        }

        private int GetSlot(EnemyTypeId typeId, byte polarity)
        {
            int polarityBit = polarity == 0 ? 0 : 1;
            return ((int)typeId * 2) + polarityBit;
        }

        private int GetSlotCount()
        {
            return typeCount * 2;
        }

        private static int GetTypeCount()
        {
            var typeValues = System.Enum.GetValues(typeof(EnemyTypeId));
            int maxTypeIndex = 0;

            foreach (EnemyTypeId typeId in typeValues)
            {
                if ((int)typeId > maxTypeIndex)
                {
                    maxTypeIndex = (int)typeId;
                }
            }

            return maxTypeIndex + 1;
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
            if (enemySet == null)
            {
                Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            }

            if (playerPositionVar == null)
            {
                Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            }

            if (quadMesh == null)
            {
                Debug.LogWarning($"[{GetType().Name}] quadMesh not assigned on {gameObject.name}.", this);
            }

            if (baseMaterial == null)
            {
                Debug.LogWarning($"[{GetType().Name}] baseMaterial not assigned on {gameObject.name}.", this);
            }

            if (visualConfig == null)
            {
                Debug.LogWarning($"[{GetType().Name}] visualConfig not assigned on {gameObject.name}.", this);
            }
        }
#endif
    }
}
