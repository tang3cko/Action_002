using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using Action002.Audio.Systems;
using Action002.Boss.Data;
using Action002.Boss.Logic;

namespace Action002.Tests.Boss
{
    public class BossAITests
    {
        private MockBossActions mock;
        private MockRhythmClock mockClock;
        private BossAI ai;

        private class MockRhythmClock : IRhythmClock
        {
            public int HalfBeatIndex;
            public bool Playing = false;

            public bool StartClock() { Playing = true; return true; }
            public void StopClock() { Playing = false; }
            public void ProcessClock() { }
            public void ResetForNewRun() { HalfBeatIndex = 0; Playing = false; }

            public bool ShouldFireOnDownbeat(ref int lastConsumedIndex)
            {
                if (!Playing) return false;
                if (HalfBeatIndex <= lastConsumedIndex) return false;
                if (HalfBeatIndex % 2 != 0) return false;
                lastConsumedIndex = HalfBeatIndex;
                return true;
            }

            public bool ShouldFireOnOffbeat(ref int lastConsumedIndex)
            {
                if (!Playing) return false;
                if (HalfBeatIndex <= lastConsumedIndex) return false;
                if (HalfBeatIndex % 2 == 0) return false;
                lastConsumedIndex = HalfBeatIndex;
                return true;
            }

            public void AdvanceToDownbeat()
            {
                HalfBeatIndex += (HalfBeatIndex % 2 == 0) ? 2 : 1;
            }

            public void AdvanceToOffbeat()
            {
                HalfBeatIndex += (HalfBeatIndex % 2 != 0) ? 2 : 1;
            }
        }

        private class MockBossActions : IBossAIActions
        {
            public List<string> Calls = new();
            public List<(BossEntityId Id, float X, float Y)> SpawnedEntities = new();
            public List<(BossEntityId Id, float X, float Y, byte Polarity, int Pattern)> FiredBullets = new();
            public List<int> ScoresAdded = new();
            public int ForcedSwitchCount;
            public bool BossDefeatedNotified;
            public bool MergeAnimationPlayed;

            public void SpawnEntity(BossEntityId id, float x, float y, int hp, byte polarity, float collisionRadius)
            {
                Calls.Add("SpawnEntity");
                SpawnedEntities.Add((id, x, y));
            }
            public void DespawnEntity(BossEntityId id) => Calls.Add("DespawnEntity");
            public void SetEntityPosition(BossEntityId id, float x, float y) => Calls.Add("SetEntityPosition");
            public void SetEntityActive(BossEntityId id, bool active) => Calls.Add("SetEntityActive");
            public void FireBullets(BossEntityId sourceId, float x, float y, byte polarity, int patternIndex)
            {
                Calls.Add("FireBullets");
                FiredBullets.Add((sourceId, x, y, polarity, patternIndex));
            }
            public void RequestForcedPolaritySwitch()
            {
                Calls.Add("RequestForcedPolaritySwitch");
                ForcedSwitchCount++;
            }
            public void AddScore(int amount)
            {
                Calls.Add("AddScore");
                ScoresAdded.Add(amount);
            }
            public void NotifyBossDefeated()
            {
                Calls.Add("NotifyBossDefeated");
                BossDefeatedNotified = true;
            }
            public void PlayMergeAnimation()
            {
                Calls.Add("PlayMergeAnimation");
                MergeAnimationPlayed = true;
            }
        }

        private BossAI CreateAI(
            IBossAIActions actions,
            IRhythmClock clock = null,
            int guardianHp = 10, int magatamaHp = 20,
            float phase1ShootCooldown = 0.5f, float phase2ShootCooldown = 0.15f,
            float simultaneousThreshold = 5f,
            float phase1SwitchInterval = 10f, float phase2SwitchInterval = 5f,
            float warningDuration = 2f,
            float introDuration = 1f, float mergeDuration = 2f,
            int phase1KillScore = 500, int phase2KillScore = 1000)
        {
            return new BossAI(
                actions,
                clock ?? mockClock,
                phase1HpPerGuardian: guardianHp,
                phase2HpMagatama: magatamaHp,
                phase1ShootCooldown: phase1ShootCooldown,
                phase2ShootCooldown: phase2ShootCooldown,
                phase1SimultaneousThreshold: simultaneousThreshold,
                phase1ForcedSwitchInterval: phase1SwitchInterval,
                phase2ForcedSwitchInterval: phase2SwitchInterval,
                forcedSwitchWarningDuration: warningDuration,
                introDuration: introDuration,
                mergeDuration: mergeDuration,
                phase1KillScore: phase1KillScore,
                phase2KillScore: phase2KillScore,
                guardianCollisionRadius: 1.0f,
                magatamaCollisionRadius: 1.5f,
                whiteGuardianOffset: new float2(-3f, 3f),
                blackGuardianOffset: new float2(3f, 3f),
                magatamaRotationSpeed: 0.5f);
        }

        [SetUp]
        public void SetUp()
        {
            mock = new MockBossActions();
            mockClock = new MockRhythmClock();
            mockClock.Playing = true;
            ai = CreateAI(mock);
        }

        // --- Begin ---

        [Test]
        public void Begin_EntersIntroPhase()
        {
            ai.Begin(float2.zero);
            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Intro));
        }

        [Test]
        public void Begin_SpawnsBothGuardians()
        {
            ai.Begin(float2.zero);
            Assert.That(mock.SpawnedEntities.Count, Is.EqualTo(2));
            Assert.That(mock.SpawnedEntities[0].Id, Is.EqualTo(BossEntityId.WhiteGuardian));
            Assert.That(mock.SpawnedEntities[1].Id, Is.EqualTo(BossEntityId.BlackGuardian));
        }

        // --- Intro → Phase1 ---

        [Test]
        public void Intro_TransitionsToPhase1AfterDuration()
        {
            ai.Begin(float2.zero);

            ai.Tick(0.5f);
            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Intro));

            ai.Tick(0.6f);
            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Phase1));
        }

        [Test]
        public void Intro_RequiresSeparateTick_ForPhase1Behavior()
        {
            ai = CreateAI(mock, introDuration: 0f);
            ai.Begin(float2.zero);

            // Tick 1: transitions Intro → Phase1, but Phase1 attack NOT run
            ai.Tick(0.1f);
            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Phase1));
            int bulletsBeforePhase1Tick = mock.FiredBullets.Count;

            // Tick 2: Phase1 runs attacks (with downbeat available)
            mockClock.AdvanceToDownbeat();
            ai.Tick(0.6f);
            Assert.That(mock.FiredBullets.Count, Is.GreaterThan(bulletsBeforePhase1Tick));
        }

        // --- Phase1 Attack ---

        [Test]
        public void Phase1_FiresBulletsOnDownbeat()
        {
            ai = CreateAI(mock, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            mockClock.AdvanceToDownbeat();
            ai.Tick(0.6f); // Phase1 fires on downbeat

            Assert.That(mock.FiredBullets.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Phase1_DoesNotFireWithoutDownbeat()
        {
            ai = CreateAI(mock, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            // No clock advance — no downbeat available
            ai.Tick(0.6f);

            Assert.That(mock.FiredBullets.Count, Is.EqualTo(0));
        }

        [Test]
        public void Phase1_DoesNotFireOnOffbeat()
        {
            ai = CreateAI(mock, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            mockClock.AdvanceToOffbeat(); // offbeatを進める
            ai.Tick(0.6f);

            Assert.That(mock.FiredBullets.Count, Is.EqualTo(0));
        }

        // --- Phase1 → Merge ---

        [Test]
        public void Phase1_BothGuardiansDefeated_TransitionsToMerge()
        {
            ai = CreateAI(mock, guardianHp: 1, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);
            ai.ApplyDamage(BossEntityId.BlackGuardian, 1);
            ai.Tick(0.1f); // Phase1 detects both dead → Merge

            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Merge));
            Assert.That(mock.MergeAnimationPlayed, Is.True);
        }

        [Test]
        public void Phase1_OneGuardianDefeated_StaysInPhase1()
        {
            ai = CreateAI(mock, guardianHp: 1, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);
            ai.Tick(0.1f);

            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Phase1));
            Assert.That(ai.GetEntity(BossEntityId.WhiteGuardian).IsActive, Is.False);
            Assert.That(ai.GetEntity(BossEntityId.BlackGuardian).IsActive, Is.True);
        }

        // --- Score: single path only ---

        [Test]
        public void ApplyDamage_GuardianKilled_AddsScoreOnce()
        {
            ai = CreateAI(mock, guardianHp: 1, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);

            Assert.That(mock.ScoresAdded.Count, Is.EqualTo(1));
            Assert.That(mock.ScoresAdded[0], Is.EqualTo(500));
        }

        [Test]
        public void ApplyDamage_NotKilled_NoScore()
        {
            ai = CreateAI(mock, guardianHp: 10, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f);

            ai.ApplyDamage(BossEntityId.WhiteGuardian, 5);

            Assert.That(mock.ScoresAdded.Count, Is.EqualTo(0));
        }

        // --- Merge → Phase2 ---

        [Test]
        public void Merge_TransitionsToPhase2AfterDuration()
        {
            ai = CreateAI(mock, guardianHp: 1, introDuration: 0f, mergeDuration: 1f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1

            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);
            ai.ApplyDamage(BossEntityId.BlackGuardian, 1);
            ai.Tick(0.1f); // Phase1 → Merge
            ai.Tick(1.1f); // Merge → Phase2

            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Phase2));
            Assert.That(mock.SpawnedEntities.Exists(e => e.Id == BossEntityId.Magatama), Is.True);
        }

        // --- Phase2 ---

        [Test]
        public void Phase2_FiresBulletsOnOffbeat()
        {
            GoToPhase2();
            int before = mock.FiredBullets.Count;

            mockClock.AdvanceToOffbeat();
            ai.Tick(0.2f);

            Assert.That(mock.FiredBullets.Count, Is.GreaterThan(before));
        }

        [Test]
        public void Phase2_DoesNotFireWithoutOffbeat()
        {
            GoToPhase2();
            int before = mock.FiredBullets.Count;

            // No clock advance
            ai.Tick(0.2f);

            Assert.That(mock.FiredBullets.Count, Is.EqualTo(before));
        }

        [Test]
        public void Phase2_DoesNotFireOnDownbeat()
        {
            GoToPhase2();
            int before = mock.FiredBullets.Count;

            mockClock.AdvanceToDownbeat(); // downbeatを進める
            ai.Tick(0.2f);

            Assert.That(mock.FiredBullets.Count, Is.EqualTo(before));
        }

        [Test]
        public void Phase2_MagatamaDefeated_TransitionsToDefeated()
        {
            GoToPhase2();
            ai.ApplyDamage(BossEntityId.Magatama, 25);
            ai.Tick(0.1f);

            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Defeated));
            Assert.That(mock.BossDefeatedNotified, Is.True);
            Assert.That(ai.IsActive, Is.False);
        }

        [Test]
        public void Phase2_MagatamaDefeated_NotifiesOnlyOnce()
        {
            GoToPhase2();
            ai.ApplyDamage(BossEntityId.Magatama, 25);
            ai.Tick(0.1f);
            ai.Tick(0.1f);

            int defeatCount = 0;
            foreach (var c in mock.Calls)
                if (c == "NotifyBossDefeated") defeatCount++;
            Assert.That(defeatCount, Is.EqualTo(1));
        }

        [Test]
        public void Phase2_MagatamaDefeated_AddsScore()
        {
            GoToPhase2();
            mock.ScoresAdded.Clear();
            ai.ApplyDamage(BossEntityId.Magatama, 25);
            ai.Tick(0.1f);

            Assert.That(mock.ScoresAdded, Does.Contain(1000));
        }

        // --- Forced Polarity Switch ---

        [Test]
        public void Phase1_ForcedSwitch_FiresAfterInterval()
        {
            ai = CreateAI(mock, introDuration: 0f, phase1SwitchInterval: 3f, warningDuration: 1f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1
            ai.Tick(3.1f); // past switch interval → warning starts

            // Warning active, not yet fired
            Assert.That(mock.ForcedSwitchCount, Is.EqualTo(0));

            ai.Tick(1.1f); // past warning → switch fires
            Assert.That(mock.ForcedSwitchCount, Is.EqualTo(1));
        }

        // --- Damage guards ---

        [Test]
        public void ApplyDamage_WrongPhase_DoesNothing()
        {
            ai.Begin(float2.zero);
            // Still in Intro
            ai.ApplyDamage(BossEntityId.WhiteGuardian, 5);
            Assert.That(ai.GetEntity(BossEntityId.WhiteGuardian).Hp, Is.EqualTo(10));
        }

        [Test]
        public void ApplyDamage_MagatamaInPhase1_DoesNothing()
        {
            ai = CreateAI(mock, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // → Phase1
            ai.ApplyDamage(BossEntityId.Magatama, 5);
            Assert.That(ai.CurrentPhase, Is.EqualTo(BossPhaseId.Phase1));
        }

        [Test]
        public void ApplyDamage_DeadGuardian_DoesNothing()
        {
            ai = CreateAI(mock, guardianHp: 1, introDuration: 0f);
            ai.Begin(float2.zero);
            ai.Tick(0.1f);
            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);
            mock.ScoresAdded.Clear();

            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);
            Assert.That(mock.ScoresAdded.Count, Is.EqualTo(0));
        }

        // --- Helper ---

        private void GoToPhase2()
        {
            ai = CreateAI(mock, guardianHp: 1, introDuration: 0f, mergeDuration: 0f, magatamaHp: 20);
            ai.Begin(float2.zero);
            ai.Tick(0.1f); // Intro → Phase1
            ai.ApplyDamage(BossEntityId.WhiteGuardian, 1);
            ai.ApplyDamage(BossEntityId.BlackGuardian, 1);
            ai.Tick(0.1f); // Phase1 → Merge
            ai.Tick(0.1f); // Merge → Phase2
        }
    }
}
