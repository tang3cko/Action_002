using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Action002.Enemy.Data
{
    [CreateAssetMenu(fileName = "EnemyDeathBuffer", menuName = "Action002/Data/Enemy/Enemy Death Buffer")]
    public class EnemyDeathBufferSO : ScriptableObject
    {
        private readonly List<EnemyDeathParticle> particles = new List<EnemyDeathParticle>(32);

        public int Count => particles.Count;

        public EnemyDeathParticle GetParticle(int index)
        {
            return particles[index];
        }

        public void Add(float2 position, byte polarity, EnemyTypeId typeId)
        {
            particles.Add(new EnemyDeathParticle
            {
                Position = position,
                Polarity = polarity,
                TypeId = typeId,
                ElapsedTime = 0f,
            });
        }

        public void AdvanceTimers(float deltaTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.ElapsedTime += deltaTime;
                particles[i] = p;
            }
        }

        public void RemoveCompleted(float duration)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (particles[i].ElapsedTime >= duration)
                {
                    particles.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            particles.Clear();
        }

        private void OnEnable()
        {
            particles.Clear();
        }

        private void OnDisable()
        {
            particles.Clear();
        }
    }
}
