using System;
using UnityEngine;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Action002.Visual;

namespace Action002.Enemy.Rendering
{
    public class EnemyDeathEffectRenderer : MonoBehaviour
    {
        private const int BATCH_SIZE = 1023;

        [Header("Dependencies")]
        [SerializeField] private EnemyDeathBufferSO deathBuffer;
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material baseMaterial;

        [Header("Settings")]
        [SerializeField] private EnemyVisualConfigSO visualConfig;

        private Material bodyMaterial;
        private Texture2D fallbackTexture;
        private MaterialPropertyBlock bodyBlock;
        private Matrix4x4[][] bodyBatches;
        private int[] bodyCounts;
        private int typeCount;

        private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_BaseMap");
        private static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");

        private void Start()
        {
            fallbackTexture = DiamondTextureGenerator.Create(64);

            if (baseMaterial != null)
            {
                bodyMaterial = Instantiate(baseMaterial);
                SetupMaterialAlphaClip(bodyMaterial, fallbackTexture);
            }

            bodyBlock = new MaterialPropertyBlock();

            Array typeValues = Enum.GetValues(typeof(EnemyTypeId));
            int maxTypeIndex = 0;
            foreach (EnemyTypeId id in typeValues)
            {
                if ((int)id > maxTypeIndex)
                    maxTypeIndex = (int)id;
            }
            typeCount = maxTypeIndex + 1;

            int slotCount = typeCount * 2;
            bodyBatches = new Matrix4x4[slotCount][];
            bodyCounts = new int[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                bodyBatches[i] = new Matrix4x4[BATCH_SIZE];
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
            if (deathBuffer == null || deathBuffer.Count == 0) return;
            if (bodyMaterial == null || quadMesh == null) return;

            deathBuffer.AdvanceTimers(Time.deltaTime);

            int slotCount = typeCount * 2;
            for (int i = 0; i < slotCount; i++)
            {
                bodyCounts[i] = 0;
            }

            for (int i = 0; i < deathBuffer.Count; i++)
            {
                var particle = deathBuffer.GetParticle(i);
                float baseScale = EnemyTypeTable.Get(particle.TypeId).VisualScale;
                float scale = EnemyDeathCalculator.CalculateScale(particle.ElapsedTime, baseScale);
                if (scale <= 0f) continue;

                int slot = GetSlot(particle.TypeId, particle.Polarity);
                bodyBatches[slot][bodyCounts[slot]++] = Matrix4x4.TRS(
                    new Vector3(particle.Position.x, particle.Position.y, 0.04f),
                    Quaternion.identity,
                    Vector3.one * scale
                );

                if (bodyCounts[slot] == BATCH_SIZE)
                {
                    FlushBody(slot);
                    bodyCounts[slot] = 0;
                }
            }

            for (int slot = 0; slot < slotCount; slot++)
            {
                if (bodyCounts[slot] > 0)
                    FlushBody(slot);
            }

            deathBuffer.RemoveCompleted(EnemyDeathCalculator.DURATION);
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

        private int GetSlot(EnemyTypeId typeId, byte polarity)
        {
            int polarityBit = polarity == 0 ? 0 : 1;
            return ((int)typeId * 2) + polarityBit;
        }

        private void FlushBody(int slot)
        {
            int typeIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForType((EnemyTypeId)typeIndex);
            bodyBlock.SetTexture(MAIN_TEX_ID, tex);
            bodyBlock.SetColor(COLOR_ID, PolarityColors.GetForeground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, bodyMaterial, bodyBatches[slot], bodyCounts[slot], bodyBlock);
        }

        private void OnDestroy()
        {
            if (fallbackTexture != null) Destroy(fallbackTexture);
            if (bodyMaterial != null) Destroy(bodyMaterial);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (deathBuffer == null) Debug.LogWarning($"[{GetType().Name}] deathBuffer not assigned on {gameObject.name}.", this);
            if (quadMesh == null) Debug.LogWarning($"[{GetType().Name}] quadMesh not assigned on {gameObject.name}.", this);
            if (baseMaterial == null) Debug.LogWarning($"[{GetType().Name}] baseMaterial not assigned on {gameObject.name}.", this);
            if (visualConfig == null) Debug.LogWarning($"[{GetType().Name}] visualConfig not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
