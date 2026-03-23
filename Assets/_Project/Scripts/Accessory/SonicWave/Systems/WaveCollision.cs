using System.Collections.Generic;
using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Logic;
using Action002.Core;
using Action002.Enemy.Data;
using Action002.Enemy.Logic;
using Tang3cko.ReactiveSO;

namespace Action002.Accessory.SonicWave.Systems
{
    public class WaveCollision
    {
        private readonly WaveStateSetSO waveSet;
        private readonly EnemyStateSetSO enemySet;
        private readonly EnemyDeathBufferSO deathBuffer;
        private readonly IntEventChannelSO onEnemyKilled;
        private readonly IntEventChannelSO onKillScoreAdded;
        private readonly GameConfigSO gameConfig;
        private readonly int killScore;

        private IBossHitTarget bossHitTarget;

        // key = waveId, value = ヒット済みターゲット ID のセット
        // 敵: enemyId そのまま（正の整数）
        // ボス entity: -(bossEntityIndex + 1) で負の整数にマッピング
        private readonly Dictionary<int, HashSet<int>> waveHitHistory = new Dictionary<int, HashSet<int>>();
        private readonly HashSet<int> enemyKillSet = new HashSet<int>();
        private readonly List<int> enemyDespawnQueue = new List<int>(64);
        private readonly List<int> expiredWaveKeys = new List<int>(16);

        public WaveCollision(
            WaveStateSetSO waveSet,
            EnemyStateSetSO enemySet,
            EnemyDeathBufferSO deathBuffer,
            IntEventChannelSO onEnemyKilled,
            IntEventChannelSO onKillScoreAdded,
            GameConfigSO gameConfig,
            int killScore = 50)
        {
            this.waveSet = waveSet;
            this.enemySet = enemySet;
            this.deathBuffer = deathBuffer;
            this.onEnemyKilled = onEnemyKilled;
            this.onKillScoreAdded = onKillScoreAdded;
            this.gameConfig = gameConfig;
            this.killScore = killScore;
        }

        public void SetBossHitTarget(IBossHitTarget target)
        {
            bossHitTarget = target;
        }

        public void ProcessCollisions()
        {
            if (waveSet == null || waveSet.Count == 0) return;
            if (gameConfig == null) return;

            float ringThickness = gameConfig.WaveRingThickness;

            // 1. waveHitHistory のキーのうち現存しない waveId を削除
            CleanupExpiredWaveHistory();

            // 2. enemyKillSet をクリア
            enemyKillSet.Clear();
            enemyDespawnQueue.Clear();

            var waveData = waveSet.Data;
            var waveIds = waveSet.EntityIds;

            for (int wi = 0; wi < waveData.Length; wi++)
            {
                var wave = waveData[wi];
                int waveId = waveIds[wi];

                if (!waveHitHistory.TryGetValue(waveId, out var hitSet))
                {
                    hitSet = new HashSet<int>();
                    waveHitHistory[waveId] = hitSet;
                }

                // a. ボス判定
                if (bossHitTarget != null && bossHitTarget.IsActive)
                {
                    int entityCount = bossHitTarget.EntityCount;
                    for (int ei = 0; ei < entityCount; ei++)
                    {
                        int targetId = -(ei + 1);
                        if (hitSet.Contains(targetId)) continue;

                        if (!bossHitTarget.GetEntityInfo(ei, out float2 entityPos,
                            out float entityRadius, out bool entityActive))
                            continue;
                        if (!entityActive) continue;

                        if (WaveCollisionCalculator.IsHit(wave, ringThickness, entityPos, entityRadius))
                        {
                            hitSet.Add(targetId);
                            bossHitTarget.TryApplyDamageToEntity(ei, wave.Damage);
                        }
                    }
                }

                // b. 全敵を走査
                if (enemySet != null && enemySet.Count > 0)
                {
                    var enemyData = enemySet.Data;
                    var enemyIds = enemySet.EntityIds;

                    for (int ei = 0; ei < enemyData.Length; ei++)
                    {
                        int enemyId = enemyIds[ei];
                        if (hitSet.Contains(enemyId)) continue;
                        if (enemyKillSet.Contains(enemyId)) continue;

                        var enemy = enemyData[ei];
                        float enemyCollisionRadius = EnemyTypeTable.Get(enemy.TypeId).CollisionRadius;

                        if (!WaveCollisionCalculator.IsHit(wave, ringThickness,
                            enemy.Position, enemyCollisionRadius))
                            continue;

                        hitSet.Add(enemyId);

                        var damageResult = EnemyDamageCalculator.ApplyDamage(enemy.Hp, wave.Damage);

                        if (!damageResult.IsKilled)
                        {
                            enemy.Hp = damageResult.RemainingHp;
                            enemySet.SetData(enemyId, enemy);
                        }
                        else
                        {
                            enemyKillSet.Add(enemyId);
                            enemyDespawnQueue.Add(enemyId);

                            if (deathBuffer != null)
                                deathBuffer.Add(enemy.Position, enemy.Polarity, enemy.TypeId);

                            onKillScoreAdded?.RaiseEvent(killScore);
                            onEnemyKilled?.RaiseEvent(1);
                        }
                    }
                }
            }

            // フレーム末尾でデスポーン
            foreach (var id in enemyDespawnQueue)
            {
                if (enemySet != null)
                    enemySet.Unregister(id);
            }
        }

        public void ResetForNewRun()
        {
            waveHitHistory.Clear();
            enemyKillSet.Clear();
            enemyDespawnQueue.Clear();
        }

        private void CleanupExpiredWaveHistory()
        {
            expiredWaveKeys.Clear();
            var waveIds = waveSet.EntityIds;

            // Build a fast lookup of current wave IDs
            var currentWaveIds = new HashSet<int>(waveIds.Length);
            for (int i = 0; i < waveIds.Length; i++)
                currentWaveIds.Add(waveIds[i]);

            foreach (var key in waveHitHistory.Keys)
            {
                if (!currentWaveIds.Contains(key))
                    expiredWaveKeys.Add(key);
            }

            foreach (var key in expiredWaveKeys)
                waveHitHistory.Remove(key);
        }
    }
}
