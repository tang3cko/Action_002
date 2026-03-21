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
            public Texture2D Texture;
            public float Size;
            public bool HasOutline;
            public float BodyZ;
            public float OutlineZ;
        }

        [SerializeField] private Entry[] entries;

        private Entry[] lookup;

        public Entry GetPolicy(BulletFaction faction)
        {
            if (lookup == null) BuildLookup();
            int index = (int)faction;
            if (index >= 0 && index < lookup.Length)
                return lookup[index];
            return default;
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

            lookup = new Entry[maxIndex + 1];

            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                int index = (int)entries[i].Faction;
                if (index >= 0 && index < lookup.Length)
                    lookup[index] = entries[i];
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

            // Check all BulletFactions are present
            foreach (BulletFaction f in values)
            {
                bool found = false;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Faction == f)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    Debug.LogWarning($"[{GetType().Name}] Missing entry for {f} on {name}.", this);
            }

            // Check key uniqueness
            for (int i = 0; i < entries.Length; i++)
            {
                for (int j = i + 1; j < entries.Length; j++)
                {
                    if (entries[i].Faction == entries[j].Faction)
                        Debug.LogWarning($"[{GetType().Name}] Duplicate entry for {entries[i].Faction} on {name}.", this);
                }
            }

            BuildLookup();
        }
#endif
    }
}
