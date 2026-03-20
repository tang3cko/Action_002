using System.Collections.Generic;
using NUnit.Framework;
using Action002.Core.Flow;

namespace Action002.Tests.Core
{
    public class GameplayStartupLogicTests
    {
        private class StubGameplayStartupActions : IGameplayStartupActions
        {
            public readonly List<string> CallLog = new List<string>();
            public bool StartClockResult;

            public StubGameplayStartupActions(bool startClockResult = true)
            {
                StartClockResult = startClockResult;
            }

            public void DisablePlayerInput() => CallLog.Add("DisablePlayerInput");
            public void EnablePlayerInput() => CallLog.Add("EnablePlayerInput");
            public void ResetForNewRun() => CallLog.Add("ResetForNewRun");

            public bool StartClock()
            {
                CallLog.Add("StartClock");
                return StartClockResult;
            }

            public void SetRunning(bool running) => CallLog.Add($"SetRunning({running})");
            public void LogStartupError(string message) => CallLog.Add($"LogStartupError({message})");
        }

        // ── StartClock succeeds ──

        [Test]
        public void Execute_WhenStartClockSucceeds_CallsAllActionsInOrder()
        {
            var stub = new StubGameplayStartupActions(startClockResult: true);
            var logic = new GameplayStartupLogic(stub);

            logic.Execute();

            var expected = new List<string>
            {
                "DisablePlayerInput",
                "ResetForNewRun",
                "StartClock",
                "EnablePlayerInput",
                "SetRunning(True)",
            };
            Assert.That(stub.CallLog, Is.EqualTo(expected));
        }

        // ── StartClock fails ──

        [Test]
        public void Execute_WhenStartClockFails_StopsAfterStartClock()
        {
            var stub = new StubGameplayStartupActions(startClockResult: false);
            var logic = new GameplayStartupLogic(stub);

            logic.Execute();

            var expected = new List<string>
            {
                "DisablePlayerInput",
                "ResetForNewRun",
                "StartClock",
                "LogStartupError([GameplayStartupLogic] StartClock failed. Gameplay will not start.)",
            };
            Assert.That(stub.CallLog, Is.EqualTo(expected));
            Assert.That(stub.CallLog, Has.No.Member("EnablePlayerInput"));
            Assert.That(stub.CallLog, Has.No.Member("SetRunning(True)"));
        }
    }
}
