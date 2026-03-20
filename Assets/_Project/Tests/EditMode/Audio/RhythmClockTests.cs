using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using Action002.Audio.Data;
using Action002.Audio.Systems;

namespace Action002.Tests.Audio
{
    public class RhythmClockTests
    {
        private RhythmClockConfigSO config;
        private RhythmClock clock;
        private double currentDspTime;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<RhythmClockConfigSO>();
            SetConfigBpm(config, 120f);
            SetConfigStartOffset(config, 0.0);
            currentDspTime = 0.0;
            clock = new RhythmClock(config, () => currentDspTime);
        }

        [TearDown]
        public void TearDown()
        {
            if (config != null)
                Object.DestroyImmediate(config);
        }

        // ── StartClock ──

        [Test]
        public void StartClock_SetsIsPlayingTrue()
        {
            bool result = clock.StartClock();

            Assert.That(result, Is.True);
            Assert.That(clock.IsPlaying, Is.True);
        }

        [Test]
        public void StartClock_NullConfig_DoesNotStart()
        {
            var nullClock = new RhythmClock(null, () => 0.0);
            LogAssert.Expect(LogType.Error, "[RhythmClock] config is null. Clock not started.");

            bool result = nullClock.StartClock();

            Assert.That(result, Is.False);
            Assert.That(nullClock.IsPlaying, Is.False);
        }

        [Test]
        public void StartClock_NullConfig_DoesNotStart_Duplicate()
        {
            // OnValidate clamps BPM to 120 when set to <= 0,
            // so we test null config to exercise the defensive guard.
            var nullClock = new RhythmClock(null, () => 0.0);
            LogAssert.Expect(LogType.Error, "[RhythmClock] config is null. Clock not started.");

            bool result = nullClock.StartClock();

            Assert.That(result, Is.False);
            Assert.That(nullClock.IsPlaying, Is.False);
        }

        [Test]
        public void StartClock_InitializesSecondsPerHalfBeat()
        {
            clock.StartClock();

            Assert.That(clock.SecondsPerHalfBeat, Is.EqualTo(0.25f));
        }

        [Test]
        public void StartClock_ResetsHalfBeatIndex()
        {
            clock.StartClock();
            currentDspTime = 1.0;
            clock.ProcessClock();
            currentDspTime = 0.0;
            clock.StartClock();

            Assert.That(clock.CurrentHalfBeatIndex, Is.EqualTo(0));
        }

        // ── StopClock ──

        [Test]
        public void StopClock_SetsIsPlayingFalse()
        {
            clock.StartClock();

            clock.StopClock();

            Assert.That(clock.IsPlaying, Is.False);
        }

        [Test]
        public void StopClock_WhenNotPlaying_RemainsFalse()
        {
            clock.StopClock();

            Assert.That(clock.IsPlaying, Is.False);
        }

        // ── ProcessClock ──

        [Test]
        public void ProcessClock_UpdatesHalfBeatIndex()
        {
            clock.StartClock();

            currentDspTime = 0.5;
            clock.ProcessClock();

            Assert.That(clock.CurrentHalfBeatIndex, Is.EqualTo(2));
        }

        [Test]
        public void ProcessClock_BeforeStartTime_DoesNotUpdate()
        {
            SetConfigStartOffset(config, 1.0);
            var offsetClock = new RhythmClock(config, () => currentDspTime);
            offsetClock.StartClock();

            currentDspTime = 0.5;
            offsetClock.ProcessClock();

            Assert.That(offsetClock.CurrentHalfBeatIndex, Is.EqualTo(0));
        }

        [Test]
        public void ProcessClock_NotPlaying_DoesNotUpdate()
        {
            currentDspTime = 1.0;
            clock.ProcessClock();

            Assert.That(clock.CurrentHalfBeatIndex, Is.EqualTo(0));
        }

        [Test]
        public void ProcessClock_AfterStartOffset_UpdatesCorrectly()
        {
            SetConfigStartOffset(config, 0.5);
            var offsetClock = new RhythmClock(config, () => currentDspTime);
            offsetClock.StartClock();

            currentDspTime = 1.0;
            offsetClock.ProcessClock();

            Assert.That(offsetClock.CurrentHalfBeatIndex, Is.EqualTo(2));
        }

        // ── ShouldFireOnDownbeat ──

        [Test]
        public void ShouldFireOnDownbeat_OnEvenIndex_ReturnsTrue()
        {
            clock.StartClock();
            currentDspTime = 0.5;
            clock.ProcessClock();
            int consumed = -1;

            bool result = clock.ShouldFireOnDownbeat(ref consumed);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldFireOnDownbeat_OnOddIndex_ReturnsFalse()
        {
            clock.StartClock();
            currentDspTime = 0.25;
            clock.ProcessClock();
            int consumed = -1;

            bool result = clock.ShouldFireOnDownbeat(ref consumed);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldFireOnDownbeat_NotPlaying_ReturnsFalse()
        {
            int consumed = -1;

            bool result = clock.ShouldFireOnDownbeat(ref consumed);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldFireOnDownbeat_AlreadyConsumed_ReturnsFalse()
        {
            clock.StartClock();
            currentDspTime = 0.5;
            clock.ProcessClock();
            int consumed = -1;
            clock.ShouldFireOnDownbeat(ref consumed);

            bool result = clock.ShouldFireOnDownbeat(ref consumed);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldFireOnDownbeat_ConsumesIndex()
        {
            clock.StartClock();
            currentDspTime = 0.5;
            clock.ProcessClock();
            int consumed = -1;

            clock.ShouldFireOnDownbeat(ref consumed);

            Assert.That(consumed, Is.EqualTo(2));
        }

        // ── ShouldFireOnOffbeat ──

        [Test]
        public void ShouldFireOnOffbeat_OnOddIndex_ReturnsTrue()
        {
            clock.StartClock();
            currentDspTime = 0.25;
            clock.ProcessClock();
            int consumed = -1;

            bool result = clock.ShouldFireOnOffbeat(ref consumed);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldFireOnOffbeat_OnEvenIndex_ReturnsFalse()
        {
            clock.StartClock();
            currentDspTime = 0.5;
            clock.ProcessClock();
            int consumed = -1;

            bool result = clock.ShouldFireOnOffbeat(ref consumed);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldFireOnOffbeat_NotPlaying_ReturnsFalse()
        {
            int consumed = -1;

            bool result = clock.ShouldFireOnOffbeat(ref consumed);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldFireOnOffbeat_AlreadyConsumed_ReturnsFalse()
        {
            clock.StartClock();
            currentDspTime = 0.25;
            clock.ProcessClock();
            int consumed = -1;
            clock.ShouldFireOnOffbeat(ref consumed);

            bool result = clock.ShouldFireOnOffbeat(ref consumed);

            Assert.That(result, Is.False);
        }

        // ── ResetForNewRun ──

        [Test]
        public void ResetForNewRun_StopsAndResetsIndices()
        {
            clock.StartClock();
            currentDspTime = 1.0;
            clock.ProcessClock();

            clock.ResetForNewRun();

            Assert.That(clock.IsPlaying, Is.False);
            Assert.That(clock.CurrentHalfBeatIndex, Is.EqualTo(0));
        }

        [Test]
        public void ResetForNewRun_AllowsRestart()
        {
            clock.StartClock();
            currentDspTime = 1.0;
            clock.ProcessClock();
            clock.ResetForNewRun();

            currentDspTime = 0.0;
            clock.StartClock();

            Assert.That(clock.IsPlaying, Is.True);
            Assert.That(clock.CurrentHalfBeatIndex, Is.EqualTo(0));
        }

        // ── Helpers ──

        private static void SetConfigBpm(RhythmClockConfigSO cfg, float bpm)
        {
            var so = new SerializedObject(cfg);
            so.FindProperty("bpm").floatValue = bpm;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetConfigStartOffset(RhythmClockConfigSO cfg, double offset)
        {
            var so = new SerializedObject(cfg);
            so.FindProperty("startOffset").doubleValue = offset;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
