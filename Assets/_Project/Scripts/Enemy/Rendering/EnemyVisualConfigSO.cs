using System;
using UnityEngine;
using Action002.Enemy.Data;

namespace Action002.Enemy.Rendering
{
    [CreateAssetMenu(fileName = "EnemyVisualConfig", menuName = "Action002/Enemy/EnemyVisualConfig")]
    public class EnemyVisualConfigSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public EnemyTypeId TypeId;
            public byte Polarity;
            public Texture2D Texture;
        }

        [SerializeField] private Entry[] entries;

        private Texture2D[] lookup;

        /// <summary>
        /// Legacy overload kept for backward compatibility.
        /// Returns the texture for polarity 0 (white).
        /// </summary>
        public Texture2D GetTexture(EnemyTypeId typeId)
        {
            return GetTexture(typeId, 0);
        }

        public Texture2D GetTexture(EnemyTypeId typeId, int polarity)
        {
            if (lookup == null) BuildLookup();
            int polarityBit = polarity == 0 ? 0 : 1;
            int index = (int)typeId * 2 + polarityBit;
            if (index >= 0 && index < lookup.Length)
                return lookup[index];
            return null;
        }

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            var values = Enum.GetValues(typeof(EnemyTypeId));
            int maxIndex = 0;
            foreach (EnemyTypeId id in values)
            {
                if ((int)id > maxIndex) maxIndex = (int)id;
            }

            int slotCount = (maxIndex + 1) * 2;
            lookup = new Texture2D[slotCount];

            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                int polarityBit = entries[i].Polarity == 0 ? 0 : 1;
                int slot = (int)entries[i].TypeId * 2 + polarityBit;
                if (slot >= 0 && slot < lookup.Length)
                    lookup[slot] = entries[i].Texture;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var values = Enum.GetValues(typeof(EnemyTypeId));

            if (entries == null || entries.Length == 0)
            {
                Debug.LogWarning($"[{GetType().Name}] entries is empty on {name}.", this);
                return;
            }

            // Check all EnemyTypeId x Polarity combinations are present
            foreach (EnemyTypeId id in values)
            {
                for (int p = 0; p <= 1; p++)
                {
                    bool found = false;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        int entryPolarityBit = entries[i].Polarity == 0 ? 0 : 1;
                        if (entries[i].TypeId == id && entryPolarityBit == p)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        Debug.LogWarning($"[{GetType().Name}] Missing entry for {id} polarity {p} on {name}.", this);
                }
            }

            // Check key uniqueness (TypeId + Polarity)
            for (int i = 0; i < entries.Length; i++)
            {
                for (int j = i + 1; j < entries.Length; j++)
                {
                    int pi = entries[i].Polarity == 0 ? 0 : 1;
                    int pj = entries[j].Polarity == 0 ? 0 : 1;
                    if (entries[i].TypeId == entries[j].TypeId && pi == pj)
                        Debug.LogWarning($"[{GetType().Name}] Duplicate entry for {entries[i].TypeId} polarity {pi} on {name}.", this);
                }
            }

            BuildLookup();
        }
#endif
    }
}
