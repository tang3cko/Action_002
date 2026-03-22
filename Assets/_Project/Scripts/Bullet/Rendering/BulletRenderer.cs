using UnityEngine;
using Action002.Bullet.Data;
using Action002.Visual;

namespace Action002.Bullet.Rendering
{
    public class BulletRenderer : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("Rendering")]
        [SerializeField] private Mesh quadMesh;
        [SerializeField] private Material baseMaterial;

        [Header("Visual Config")]
        [SerializeField] private BulletVisualConfigSO visualConfig;

        [Header("Outline")]
        [SerializeField] private float outlineScale = 1.4f;

        private const int BATCH_SIZE = 1023;

        private Material bodyMaterial;
        private Material outlineMaterial;
        private Texture2D fallbackTexture;
        private MaterialPropertyBlock bodyBlock;
        private MaterialPropertyBlock outlineBlock;

        // Per BulletFaction × Polarity batch arrays
        private Matrix4x4[][] bodyBatches;
        private Matrix4x4[][] outlineBatches;
        private int[] bodyCounts;
        private int[] outlineCounts;
        private int factionCount;

        private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
        private static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");

        private void Start()
        {
            fallbackTexture = CircleTextureGenerator.Create(64);

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

            var factionValues = System.Enum.GetValues(typeof(BulletFaction));
            int maxFactionIndex = 0;
            foreach (BulletFaction f in factionValues)
                if ((int)f > maxFactionIndex) maxFactionIndex = (int)f;
            factionCount = maxFactionIndex + 1;
            int slotCount = factionCount * 2;
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
            if (bulletSet == null || bulletSet.Count == 0) return;
            if (bodyMaterial == null || outlineMaterial == null || quadMesh == null) return;

            int slotCount = factionCount * 2;
            for (int i = 0; i < slotCount; i++)
            {
                bodyCounts[i] = 0;
                outlineCounts[i] = 0;
            }

            var data = bulletSet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];
                var faction = state.Faction;
                int factionIndex = (int)faction;
                int polarityBit = state.Polarity == 0 ? 0 : 1;
                int slot = factionIndex * 2 + polarityBit;

                var policy = GetPolicy(faction);
                float size = policy.Size;

                var bodyMatrix = Matrix4x4.TRS(
                    new Vector3(state.Position.x, state.Position.y, policy.BodyZ),
                    Quaternion.identity,
                    Vector3.one * size
                );

                bodyBatches[slot][bodyCounts[slot]++] = bodyMatrix;
                if (bodyCounts[slot] == BATCH_SIZE)
                {
                    FlushBody(slot);
                    bodyCounts[slot] = 0;
                }

                if (policy.HasOutline)
                {
                    var outMatrix = Matrix4x4.TRS(
                        new Vector3(state.Position.x, state.Position.y, policy.OutlineZ),
                        Quaternion.identity,
                        Vector3.one * (size * outlineScale)
                    );

                    outlineBatches[slot][outlineCounts[slot]++] = outMatrix;
                    if (outlineCounts[slot] == BATCH_SIZE)
                    {
                        FlushOutline(slot);
                        outlineCounts[slot] = 0;
                    }
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

        private void OnDestroy()
        {
            if (fallbackTexture != null) Destroy(fallbackTexture);
            if (bodyMaterial != null) Destroy(bodyMaterial);
            if (outlineMaterial != null) Destroy(outlineMaterial);
        }

        private void FlushBody(int slot)
        {
            int factionIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForFaction((BulletFaction)factionIndex);
            bodyBlock.SetTexture(MAIN_TEX_ID, tex);
            bodyBlock.SetColor(COLOR_ID, PolarityColors.GetForeground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, bodyMaterial, bodyBatches[slot], bodyCounts[slot], bodyBlock);
        }

        private void FlushOutline(int slot)
        {
            int factionIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForFaction((BulletFaction)factionIndex);
            outlineBlock.SetTexture(MAIN_TEX_ID, tex);
            outlineBlock.SetColor(COLOR_ID, PolarityColors.GetBackground(polarityBit));

            Graphics.DrawMeshInstanced(quadMesh, 0, outlineMaterial, outlineBatches[slot], outlineCounts[slot], outlineBlock);
        }

        private BulletVisualConfigSO.Entry GetPolicy(BulletFaction faction)
        {
            if (visualConfig != null)
            {
                var entry = visualConfig.GetPolicy(faction);
                // Guard against missing config entry (Size=0 means unconfigured)
                if (entry.Size > 0f) return entry;
            }

            // Fallback defaults
            if (faction == BulletFaction.Player)
                return new BulletVisualConfigSO.Entry { Size = 0.28f, HasOutline = false, BodyZ = 0.02f };

            return new BulletVisualConfigSO.Entry { Size = 0.4f, HasOutline = true, BodyZ = 0.08f, OutlineZ = 0.10f };
        }

        private Texture2D GetTextureForFaction(BulletFaction faction)
        {
            if (visualConfig != null)
            {
                var policy = visualConfig.GetPolicy(faction);
                if (policy.Texture != null) return policy.Texture;
            }
            return fallbackTexture;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (quadMesh == null) Debug.LogWarning($"[{GetType().Name}] quadMesh not assigned on {gameObject.name}.", this);
            if (baseMaterial == null) Debug.LogWarning($"[{GetType().Name}] baseMaterial not assigned on {gameObject.name}.", this);
            if (visualConfig == null) Debug.LogWarning($"[{GetType().Name}] visualConfig not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
