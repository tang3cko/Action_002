using UnityEngine;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
using Action002.Core;
using Tang3cko.ReactiveSO;

namespace Action002.Player.Systems
{
    public class PlayerAttackSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;
        [SerializeField] private BulletStateSetSO bulletSet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;

        private int _lastConsumedHalfBeatIndex = -1;
        private int _nextBulletId = 200000;

        public void ProcessAttacks()
        {
            if (!rhythmClock.ShouldFireOnDownbeat(ref _lastConsumedHalfBeatIndex))
                return;

            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            float2 direction = FindDirectionToNearestEnemy(playerPos);

            var bullet = new BulletState
            {
                Position = playerPos,
                Velocity = direction * gameConfig.PlayerBulletSpeed,
                ScoreValue = 0f,
                Polarity = (byte)playerPolarityVar.Value,
                Faction = 0, // Player
                Damage = 1,
            };

            bulletSet.Register(_nextBulletId++, bullet);
        }

        private float2 FindDirectionToNearestEnemy(float2 playerPos)
        {
            if (enemySet.Count == 0)
                return new float2(0f, 1f); // Fire forward (positive Y)

            var data = enemySet.Data;
            float bestDistSq = float.MaxValue;
            float2 bestDir = new float2(0f, 1f);

            for (int i = 0; i < data.Length; i++)
            {
                float2 diff = data[i].Position - playerPos;
                float distSq = math.lengthsq(diff);
                if (distSq < bestDistSq && distSq > 0.0001f)
                {
                    bestDistSq = distSq;
                    bestDir = math.normalize(diff);
                }
            }

            return bestDir;
        }

        public void ResetForNewRun()
        {
            _lastConsumedHalfBeatIndex = -1;
            _nextBulletId = 200000;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (rhythmClock == null) Debug.LogWarning($"[{GetType().Name}] rhythmClock not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (bulletSet == null) Debug.LogWarning($"[{GetType().Name}] bulletSet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
            if (playerPolarityVar == null) Debug.LogWarning($"[{GetType().Name}] playerPolarityVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
