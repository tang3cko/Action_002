using UnityEngine;
using Unity.Mathematics;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Action002.Player.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Bullet.Systems
{
    public class EnemyShootSystem : MonoBehaviour
    {
        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Settings")]
        [SerializeField] private float bulletSpeed = 3f;
        [SerializeField] private float bulletLifetime = 5f;
        [SerializeField] private float bulletScoreValue = 10f;
        [SerializeField] private float fireInterval = 2f;
        [SerializeField] private int maxBulletsPerFrame = 5;

        private float fireTimer;
        private int nextBulletId = 100000;

        public void ProcessShooting()
        {
            if (enemySet.Count == 0) return;

            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f) return;
            fireTimer = fireInterval;

            var data = enemySet.Data;
            int fired = 0;

            for (int i = 0; i < data.Length && fired < maxBulletsPerFrame; i++)
            {
                var enemy = data[i];
                float2 dir = player.Position - enemy.Position;
                float dist = math.length(dir);
                if (dist < 0.01f) continue;

                float2 velocity = (dir / dist) * bulletSpeed;

                var bulletState = new BulletState
                {
                    Position = enemy.Position,
                    Velocity = velocity,
                    Lifetime = bulletLifetime,
                    ScoreValue = bulletScoreValue,
                    Polarity = enemy.Polarity,
                };

                bulletSet.Register(nextBulletId++, bulletState);
                fired++;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (player == null) Debug.LogWarning($"[{GetType().Name}] player not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
