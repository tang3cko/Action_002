using Tang3cko.ReactiveSO;

namespace Action002.Boss.Systems
{
    public class BossFlow
    {
        private readonly VoidEventChannelSO onBossTriggerReached;
        private readonly VoidEventChannelSO onBossDefeated;
        private readonly float bossTriggerTime;

        private float elapsedTime;
        private bool hasBossSpawned;
        private bool isMonitoring;

        public BossFlow(
            VoidEventChannelSO onBossTriggerReached,
            VoidEventChannelSO onBossDefeated,
            float bossTriggerTime = 120f)
        {
            this.onBossTriggerReached = onBossTriggerReached;
            this.onBossDefeated = onBossDefeated;
            this.bossTriggerTime = bossTriggerTime;
        }

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
            onBossDefeated?.RaiseEvent();
        }

        public void ResetState()
        {
            elapsedTime = 0f;
            hasBossSpawned = false;
            isMonitoring = false;
        }

        public void ProcessBossCheck(float deltaTime)
        {
            if (!isMonitoring) return;
            if (hasBossSpawned) return;

            elapsedTime += deltaTime;

            if (elapsedTime >= bossTriggerTime)
            {
                hasBossSpawned = true;
                isMonitoring = false;
                onBossTriggerReached?.RaiseEvent();
            }
        }
    }
}
