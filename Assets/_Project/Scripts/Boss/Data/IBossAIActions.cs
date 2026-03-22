namespace Action002.Boss.Data
{
    public interface IBossAIActions
    {
        void SpawnEntity(BossEntityId id, float x, float y, int hp, byte polarity, float collisionRadius);
        void DespawnEntity(BossEntityId id);
        void SetEntityPosition(BossEntityId id, float x, float y);
        void SetEntityActive(BossEntityId id, bool active);
        void FireBullets(BossEntityId sourceId, float x, float y, byte polarity, int patternIndex);
        void RequestForcedPolaritySwitch();
        void AddScore(int amount);
        void NotifyBossDefeated();
        void PlayMergeAnimation();
    }
}
