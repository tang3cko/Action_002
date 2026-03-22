using UnityEngine;
using Unity.Mathematics;
using Action002.Core;
using Action002.Enemy.Data;
using Tang3cko.ReactiveSO;

namespace Action002.Enemy.Systems
{
    public class EnemySpawnSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Sets")]
        [SerializeField] private EnemyStateSetSO enemySet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;

        private EnemySpawn logic;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void ProcessSpawning()
        {
            if (logic == null) return;

            UpdateWorldBounds();
            logic.ProcessSpawning(Time.deltaTime);
        }

        public void ResetForNewRun(uint runSeed)
        {
            if (logic == null)
            {
                logic = new EnemySpawn(gameConfig, enemySet, playerPositionVar, runSeed);
                logic.SetActive(true);
                UpdateWorldBounds();
            }
            else
            {
                logic.ResetForNewRun(runSeed);
            }
        }

        private void UpdateWorldBounds()
        {
            if (mainCamera == null) return;

            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
            logic.SetWorldBounds(new float4(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null) Debug.LogWarning($"[{GetType().Name}] gameConfig not assigned on {gameObject.name}.", this);
            if (enemySet == null) Debug.LogWarning($"[{GetType().Name}] enemySet not assigned on {gameObject.name}.", this);
            if (playerPositionVar == null) Debug.LogWarning($"[{GetType().Name}] playerPositionVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
