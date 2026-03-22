using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Boss.Systems
{
    public class BossFlowController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float bossTriggerTime = 120f;

        [Header("Variables")]
        [SerializeField] private IntVariableSO gamePhaseVar;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onBossTriggerReached;
        [SerializeField] private VoidEventChannelSO onBossDefeated;

        private float elapsedTime;
        private bool hasBossSpawned = false;
        private bool isMonitoring = false;

        // ── Unity Lifecycle ─────────────────────────────

        private void Start()
        {
            if (gamePhaseVar != null && gamePhaseVar.Value == (int)Action002.Core.Flow.GamePhase.Stage)
            {
                StartMonitoring();
            }
        }

        private void Update()
        {
            if (!isMonitoring) return;

            ProcessBossCheck(Time.deltaTime);
        }

        // ── Public Methods ──────────────────────────────

        public void StartMonitoring()
        {
            elapsedTime = 0f;
            hasBossSpawned = false;
            isMonitoring = true;
        }

        public void StopMonitoring()
        {
            isMonitoring = false;
        }

        public void NotifyBossDefeated()
        {
            if (onBossDefeated != null)
                onBossDefeated.RaiseEvent();
        }

        public void ResetState()
        {
            elapsedTime = 0f;
            hasBossSpawned = false;
            isMonitoring = false;
        }

        // ── Private Methods ─────────────────────────────

        private void ProcessBossCheck(float deltaTime)
        {
            if (hasBossSpawned) return;

            elapsedTime += deltaTime;

            if (elapsedTime >= bossTriggerTime)
            {
                hasBossSpawned = true;
                isMonitoring = false;

                if (onBossTriggerReached != null)
                    onBossTriggerReached.RaiseEvent();
            }
        }

        // ── Editor Only ─────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gamePhaseVar == null) Debug.LogWarning($"[{GetType().Name}] gamePhaseVar not assigned on {gameObject.name}.", this);
            if (onBossTriggerReached == null) Debug.LogWarning($"[{GetType().Name}] onBossTriggerReached not assigned on {gameObject.name}.", this);
            if (onBossDefeated == null) Debug.LogWarning($"[{GetType().Name}] onBossDefeated not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
