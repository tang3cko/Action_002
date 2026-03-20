using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Bullet.Logic;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
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

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;

        [Header("Settings")]
        [SerializeField] private int maxBulletsPerOffbeat = 100;

        private int lastConsumedHalfBeatIndex = -1;
        private int nextBulletId = 100000;
        private readonly Dictionary<int, float> lastShotTimes = new Dictionary<int, float>(256);

        public void ProcessShooting()
        {
            if (rhythmClock == null || enemySet == null || bulletSet == null || playerPositionVar == null) return;
            if (enemySet.Count == 0) return;

            if (!rhythmClock.ShouldFireOnOffbeat(ref lastConsumedHalfBeatIndex))
                return;

            var data = enemySet.Data;
            var entityIds = enemySet.EntityIds;
            float now = Time.time;
            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            int fired = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (fired >= maxBulletsPerOffbeat) break;

                int enemyId = entityIds[i];
                var enemy = data[i];
                var spec = EnemyTypeTable.Get(enemy.TypeId);

                if (lastShotTimes.TryGetValue(enemyId, out float lastTime)
                    && now - lastTime < spec.ShootCooldown)
                    continue;

                int remaining = maxBulletsPerOffbeat - fired;
                if (spec.ShotPattern.Count > remaining) continue;

                Span<BulletState> buf = stackalloc BulletState[spec.ShotPattern.Count];
                int written = ShotPatternCalculator.Calculate(buf, spec.ShotPattern, enemy.Position, playerPos, enemy.Polarity, spec.ScoreValue);

                if (written == 0) continue;

                for (int j = 0; j < written; j++)
                {
                    bulletSet.Register(nextBulletId++, buf[j]);
                }

                lastShotTimes[enemyId] = now;
                fired += written;
            }
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextBulletId = 100000;
            lastShotTimes.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
