using System.Collections.Generic;

namespace Action002.Bullet.Logic
{
    public class EnemyContactSessionTracker
    {
        private HashSet<int> currentContacts = new HashSet<int>();
        private HashSet<int> nextContacts = new HashSet<int>();

        public List<int> UpdateContacts(IReadOnlyList<int> contactEnemyIds)
        {
            nextContacts.Clear();
            var newContacts = new List<int>();
            for (int i = 0; i < contactEnemyIds.Count; i++)
            {
                int id = contactEnemyIds[i];
                nextContacts.Add(id);
                if (!currentContacts.Contains(id))
                    newContacts.Add(id);
            }
            var temp = currentContacts;
            currentContacts = nextContacts;
            nextContacts = temp;
            return newContacts;
        }

        public void Reset()
        {
            currentContacts.Clear();
            nextContacts.Clear();
        }
    }
}
