using UnityEngine;
using Action002.Audio.Systems;
using Action002.Bullet.Data;
using Action002.Enemy.Data;
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

        private EnemyShoot logic;

        private void Awake()
        {
            logic = new EnemyShoot(rhythmClock, enemySet, bulletSet, playerPositionVar, maxBulletsPerOffbeat);
        }

        public void ProcessShooting()
        {
            logic.ProcessShooting(Time.time);
        }

        public void ResetForNewRun()
        {
            logic.ResetForNewRun();
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
