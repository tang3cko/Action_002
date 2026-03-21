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
            public Texture2D Texture;
        }

        [SerializeField] private Entry[] entries;

        private Texture2D[] lookup;

        public Texture2D GetTexture(EnemyTypeId typeId)
        {
            if (lookup == null) BuildLookup();
            int index = (int)typeId;
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

            lookup = new Texture2D[maxIndex + 1];

            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                int index = (int)entries[i].TypeId;
                if (index >= 0 && index < lookup.Length)
                    lookup[index] = entries[i].Texture;
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

            // Check all EnemyTypeIds are present
            foreach (EnemyTypeId id in values)
            {
                bool found = false;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].TypeId == id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    Debug.LogWarning($"[{GetType().Name}] Missing entry for {id} on {name}.", this);
            }

            // Check key uniqueness
            for (int i = 0; i < entries.Length; i++)
            {
                for (int j = i + 1; j < entries.Length; j++)
                {
                    if (entries[i].TypeId == entries[j].TypeId)
                        Debug.LogWarning($"[{GetType().Name}] Duplicate entry for {entries[i].TypeId} on {name}.", this);
                }
            }

            BuildLookup();
        }
#endif
    }
}
