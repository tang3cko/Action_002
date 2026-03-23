using System;
using UnityEngine;
using Action002.Bullet.Data;

namespace Action002.Bullet.Rendering
{
    [CreateAssetMenu(fileName = "BulletVisualConfig", menuName = "Action002/Bullet/BulletVisualConfig")]
    public class BulletVisualConfigSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public BulletFaction Faction;
            public byte Polarity;
            public Texture2D Texture;
            public float Size;
            public float BodyZ;
        }

        [SerializeField] private Entry[] entries;

        private Entry[] lookup;

        public Entry GetPolicy(BulletFaction faction, int polarity)
        {
            if (lookup == null) BuildLookup();
            int polarityBit = polarity == 0 ? 0 : 1;
            int index = (int)faction * 2 + polarityBit;
            if (index >= 0 && index < lookup.Length)
                return lookup[index];
            return default;
        }

        public Texture2D GetTexture(BulletFaction faction, int polarity)
        {
            if (lookup == null) BuildLookup();
            int polarityBit = polarity == 0 ? 0 : 1;
            int index = (int)faction * 2 + polarityBit;
            if (index >= 0 && index < lookup.Length)
                return lookup[index].Texture;
            return null;
        }

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            var values = Enum.GetValues(typeof(BulletFaction));
            int maxIndex = 0;
            foreach (BulletFaction f in values)
            {
                if ((int)f > maxIndex) maxIndex = (int)f;
            }

            int slotCount = (maxIndex + 1) * 2;
            lookup = new Entry[slotCount];

            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                int polarityBit = entries[i].Polarity == 0 ? 0 : 1;
                int slot = (int)entries[i].Faction * 2 + polarityBit;
                if (slot >= 0 && slot < lookup.Length)
                    lookup[slot] = entries[i];
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var values = Enum.GetValues(typeof(BulletFaction));

            if (entries == null || entries.Length == 0)
            {
                Debug.LogWarning($"[{GetType().Name}] entries is empty on {name}.", this);
                return;
            }

            // Check all BulletFaction x Polarity combinations are present
            foreach (BulletFaction f in values)
            {
                for (int p = 0; p <= 1; p++)
                {
                    bool found = false;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        int entryPolarityBit = entries[i].Polarity == 0 ? 0 : 1;
                        if (entries[i].Faction == f && entryPolarityBit == p)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        Debug.LogWarning($"[{GetType().Name}] Missing entry for {f} polarity {p} on {name}.", this);
                }
            }

            // Check key uniqueness (Faction + Polarity)
            for (int i = 0; i < entries.Length; i++)
            {
                for (int j = i + 1; j < entries.Length; j++)
                {
                    int pi = entries[i].Polarity == 0 ? 0 : 1;
                    int pj = entries[j].Polarity == 0 ? 0 : 1;
                    if (entries[i].Faction == entries[j].Faction && pi == pj)
                        Debug.LogWarning($"[{GetType().Name}] Duplicate entry for {entries[i].Faction} polarity {pi} on {name}.", this);
                }
            }

            BuildLookup();
        }
#endif
    }
}
