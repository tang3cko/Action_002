using NUnit.Framework;
using UnityEngine;
using Action002.Boss.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Tests.Boss
{
    public class BossFlowTests
    {
        private VoidEventChannelSO onBossTriggerReached;
        private VoidEventChannelSO onBossDefeated;
        private BossFlow flow;

        [SetUp]
        public void SetUp()
        {
            onBossTriggerReached = ScriptableObject.CreateInstance<VoidEventChannelSO>();
            onBossDefeated = ScriptableObject.CreateInstance<VoidEventChannelSO>();
            flow = new BossFlow(onBossTriggerReached, onBossDefeated, bossTriggerTime: 10f);
        }

        [TearDown]
        public void TearDown()
        {
            if (onBossTriggerReached != null) Object.DestroyImmediate(onBossTriggerReached);
            if (onBossDefeated != null) Object.DestroyImmediate(onBossDefeated);
        }

        // ── ProcessBossCheck ──

        [Test]
        public void ProcessBossCheck_NotMonitoring_DoesNothing()
        {
            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;

            flow.ProcessBossCheck(100f);

            Assert.That(fired, Is.False);
        }

        [Test]
        public void ProcessBossCheck_BelowThreshold_DoesNotTrigger()
        {
            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.StartMonitoring();

            flow.ProcessBossCheck(5f);

            Assert.That(fired, Is.False);
        }

        [Test]
        public void ProcessBossCheck_ReachesThreshold_FiresEvent()
        {
            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.StartMonitoring();

            flow.ProcessBossCheck(10f);

            Assert.That(fired, Is.True);
        }

        [Test]
        public void ProcessBossCheck_NewMonitoringSession_AllowsFiringAgain()
        {
            int fireCount = 0;
            onBossTriggerReached.OnEventRaised += () => fireCount++;
            flow.StartMonitoring();
            flow.ProcessBossCheck(10f);

            flow.StartMonitoring();
            flow.ProcessBossCheck(10f);

            Assert.That(fireCount, Is.EqualTo(2));
        }

        [Test]
        public void ProcessBossCheck_AccumulatesDeltaTime()
        {
            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.StartMonitoring();

            flow.ProcessBossCheck(3f);
            flow.ProcessBossCheck(3f);
            flow.ProcessBossCheck(3f);

            Assert.That(fired, Is.False);

            flow.ProcessBossCheck(2f);

            Assert.That(fired, Is.True);
        }

        [Test]
        public void ProcessBossCheck_StopsMonitoringAfterTrigger()
        {
            int fireCount = 0;
            onBossTriggerReached.OnEventRaised += () => fireCount++;
            flow.StartMonitoring();
            flow.ProcessBossCheck(10f);

            flow.ProcessBossCheck(10f);

            Assert.That(fireCount, Is.EqualTo(1));
        }

        // ── StartMonitoring ──

        [Test]
        public void StartMonitoring_ResetsAndActivates()
        {
            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;

            flow.StartMonitoring();
            flow.ProcessBossCheck(5f);

            Assert.That(fired, Is.False);
        }

        [Test]
        public void StartMonitoring_ResetsElapsedTime()
        {
            flow.StartMonitoring();
            flow.ProcessBossCheck(9f);
            flow.StartMonitoring();

            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.ProcessBossCheck(2f);

            Assert.That(fired, Is.False);
        }

        // ── StopMonitoring ──

        [Test]
        public void StopMonitoring_Deactivates()
        {
            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.StartMonitoring();

            flow.StopMonitoring();
            flow.ProcessBossCheck(100f);

            Assert.That(fired, Is.False);
        }

        // ── ResetState ──

        [Test]
        public void ResetState_ClearsAllState()
        {
            flow.StartMonitoring();
            flow.ProcessBossCheck(9f);

            flow.ResetState();

            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.ProcessBossCheck(100f);

            Assert.That(fired, Is.False);
        }

        [Test]
        public void ResetState_AllowsFreshStart()
        {
            flow.StartMonitoring();
            flow.ProcessBossCheck(10f);
            flow.ResetState();

            bool fired = false;
            onBossTriggerReached.OnEventRaised += () => fired = true;
            flow.StartMonitoring();
            flow.ProcessBossCheck(10f);

            Assert.That(fired, Is.True);
        }

        // ── NotifyBossDefeated ──

        [Test]
        public void NotifyBossDefeated_FiresEvent()
        {
            bool fired = false;
            onBossDefeated.OnEventRaised += () => fired = true;

            flow.NotifyBossDefeated();

            Assert.That(fired, Is.True);
        }

        [Test]
        public void NotifyBossDefeated_NullChannel_DoesNotThrow()
        {
            var safeFlow = new BossFlow(onBossTriggerReached, null);

            Assert.DoesNotThrow(() => safeFlow.NotifyBossDefeated());
        }
    }
}
