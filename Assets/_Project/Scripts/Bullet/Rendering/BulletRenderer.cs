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

        private const int BATCH_SIZE = 1023;

        private Material bodyMaterial;
        private Texture2D fallbackTexture;
        private MaterialPropertyBlock bodyBlock;

        // Per BulletFaction x Polarity batch arrays
        private Matrix4x4[][] bodyBatches;
        private int[] bodyCounts;
        private int factionCount;
        private int slotCount;

        // Pre-cached policy per slot (faction x polarity)
        private BulletVisualConfigSO.Entry[] cachedPolicies;

        private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_BaseMap");

        private void Start()
        {
            fallbackTexture = CircleTextureGenerator.Create(64);

            if (baseMaterial != null)
            {
                bodyMaterial = Instantiate(baseMaterial);
                SetupMaterialAlphaClip(bodyMaterial, fallbackTexture);
            }

            bodyBlock = new MaterialPropertyBlock();

            var factionValues = System.Enum.GetValues(typeof(BulletFaction));
            int maxFactionIndex = 0;
            foreach (BulletFaction f in factionValues)
                if ((int)f > maxFactionIndex) maxFactionIndex = (int)f;
            factionCount = maxFactionIndex + 1;
            slotCount = factionCount * 2;
            bodyBatches = new Matrix4x4[slotCount][];
            bodyCounts = new int[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                bodyBatches[i] = new Matrix4x4[BATCH_SIZE];
            }

            // Pre-cache policies per slot to avoid per-bullet lookup in LateUpdate
            cachedPolicies = new BulletVisualConfigSO.Entry[slotCount];
            for (int fi = 0; fi < factionCount; fi++)
            {
                for (int pi = 0; pi <= 1; pi++)
                {
                    int slot = fi * 2 + pi;
                    cachedPolicies[slot] = GetPolicy((BulletFaction)fi, pi);
                }
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
            if (bodyMaterial == null || quadMesh == null) return;

            for (int i = 0; i < slotCount; i++)
            {
                bodyCounts[i] = 0;
            }

            var data = bulletSet.Data;

            for (int i = 0; i < data.Length; i++)
            {
                var state = data[i];
                var faction = state.Faction;
                int factionIndex = (int)faction;
                int polarityBit = state.Polarity == 0 ? 0 : 1;
                int slot = factionIndex * 2 + polarityBit;

                var policy = cachedPolicies[slot];
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
            }

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
        }

        private void FlushBody(int slot)
        {
            int factionIndex = slot / 2;
            int polarityBit = slot % 2;

            Texture2D tex = GetTextureForPolarity((BulletFaction)factionIndex, polarityBit);
            bodyBlock.Clear();
            bodyBlock.SetTexture(MAIN_TEX_ID, tex);

            Graphics.DrawMeshInstanced(quadMesh, 0, bodyMaterial, bodyBatches[slot], bodyCounts[slot], bodyBlock);
        }

        private BulletVisualConfigSO.Entry GetPolicy(BulletFaction faction, int polarityBit)
        {
            if (visualConfig != null)
            {
                var entry = visualConfig.GetPolicy(faction, polarityBit);
                // Guard against missing config entry (Size=0 means unconfigured)
                if (entry.Size > 0f) return entry;
            }

            // Fallback defaults
            if (faction == BulletFaction.Player)
                return new BulletVisualConfigSO.Entry { Size = 0.28f, BodyZ = 0.02f };

            return new BulletVisualConfigSO.Entry { Size = 0.4f, BodyZ = 0.08f };
        }

        private Texture2D GetTextureForPolarity(BulletFaction faction, int polarityBit)
        {
            if (visualConfig != null)
            {
                var tex = visualConfig.GetTexture(faction, polarityBit);
                if (tex != null) return tex;
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
