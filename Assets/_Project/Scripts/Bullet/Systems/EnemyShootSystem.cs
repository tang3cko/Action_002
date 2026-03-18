using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Action002.Audio.Systems;
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

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Settings")]
        [SerializeField] private float bulletSpeed = 3f;
        [SerializeField] private float bulletLifetime = 5f;
        [SerializeField] private float bulletScoreValue = 10f;
        [SerializeField] private int maxBulletsPerOffbeat = 100;
        [SerializeField] private float enemyShootCooldown = 1f;

        private int _lastConsumedHalfBeatIndex = -1;
        private int nextBulletId = 100000;
        private readonly Dictionary<int, float> _lastShotTimes = new Dictionary<int, float>(256);

        public void ProcessShooting()
        {
            if (rhythmClock == null) return;
            if (enemySet.Count == 0) return;

            if (!rhythmClock.ShouldFireOnOffbeat(ref _lastConsumedHalfBeatIndex))
                return;

            var data = enemySet.Data;
            var entityIds = enemySet.EntityIds;
            float now = Time.time;
            int fired = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (fired >= maxBulletsPerOffbeat) break;

                int enemyId = entityIds[i];

                // Per-enemy cooldown check
                if (_lastShotTimes.TryGetValue(enemyId, out float lastTime)
                    && now - lastTime < enemyShootCooldown)
                    continue;

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
                    Faction = 1, // Enemy
                    Damage = 1,
                };

                bulletSet.Register(nextBulletId++, bulletState);
                _lastShotTimes[enemyId] = now;
                fired++;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (player == null) Debug.LogWarning($"[{GetType().Name}] player not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
