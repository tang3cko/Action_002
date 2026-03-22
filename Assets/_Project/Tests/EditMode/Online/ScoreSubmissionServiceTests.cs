using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Action002.Core.Events;
using Action002.Online;

namespace Action002.Tests.Online
{
    public class ScoreSubmissionServiceTests
    {
        private readonly List<UnityEngine.Object> disposables = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                    UnityEngine.Object.DestroyImmediate(disposables[i]);
            }
            disposables.Clear();
        }

        private ScoreSubmissionService CreateService(IScoreBoardSender fakeSender)
        {
            var go = new GameObject("TestScoreSubmission");
            disposables.Add(go);
            var service = go.AddComponent<ScoreSubmissionService>();
            service.InjectSender(fakeSender);
            return service;
        }

        #region Send Filtering

        [Test]
        public void Submit_WhenScoreZero_ShouldNotSendToScoreBoard()
        {
            var fake = new FakeScoreBoardSender();
            var service = CreateService(fake);

            service.SendToTarget(new ScoreSubmitRequest(0, 5));

            Assert.AreEqual(0, fake.GetCallCount(1));
        }

        [Test]
        public void Submit_WhenComboZero_ShouldNotSendToComboBoard()
        {
            var fake = new FakeScoreBoardSender();
            var service = CreateService(fake);

            service.SendToTarget(new ScoreSubmitRequest(100, 0));

            Assert.AreEqual(0, fake.GetCallCount(2));
        }

        [Test]
        public void Submit_WhenBothPositive_ShouldSendToBothBoards()
        {
            var fake = new FakeScoreBoardSender();
            var service = CreateService(fake);

            service.SendToTarget(new ScoreSubmitRequest(100, 5));

            Assert.AreEqual(1, fake.GetCallCount(1), "Score board should receive one call");
            Assert.AreEqual(1, fake.GetCallCount(2), "Combo board should receive one call");
            Assert.AreEqual(100f, fake.GetLastScore(1));
            Assert.AreEqual(5f, fake.GetLastScore(2));
        }

        #endregion

        #region Error Handling

        [Test]
        public void Submit_WhenFirstBoardFails_ShouldStillSendToSecondBoard()
        {
            var fake = new FakeScoreBoardSender();
            fake.SetFailForBoard(1, new Exception("Score board error"));
            var service = CreateService(fake);

            service.SendToTarget(new ScoreSubmitRequest(100, 5));

            Assert.AreEqual(1, fake.GetCallCount(1), "Score board should have been attempted");
            Assert.AreEqual(1, fake.GetCallCount(2), "Combo board should still be called after first failure");
        }

        #endregion

        #region Pending Queue

        [Test]
        public void Submit_WhenThreeRequestsArriveWhileSubmitting_ShouldProcessAllInOrder()
        {
            var sendOrder = new List<float>();
            var fake = new RecordingScoreBoardSender(sendOrder, boardFilter: 1);
            var service = CreateService(fake);

            // 1件目送信開始（同期Fakeなので即完了するが、後続はFIFOでキューに積まれたことを検証）
            service.SendToTarget(new ScoreSubmitRequest(100, 0));
            service.SendToTarget(new ScoreSubmitRequest(200, 0));
            service.SendToTarget(new ScoreSubmitRequest(300, 0));

            Assert.AreEqual(3, sendOrder.Count, "All three requests should be processed");
            Assert.AreEqual(100f, sendOrder[0], "First request should be processed first");
            Assert.AreEqual(200f, sendOrder[1], "Second request should be processed second");
            Assert.AreEqual(300f, sendOrder[2], "Third request should be processed third");
        }

        #endregion

        private class FakeScoreBoardSender : IScoreBoardSender
        {
            private readonly Dictionary<int, int> callCounts = new Dictionary<int, int>();
            private readonly Dictionary<int, float> lastScores = new Dictionary<int, float>();
            private readonly Dictionary<int, Exception> failures = new Dictionary<int, Exception>();

            public void SetFailForBoard(int boardId, Exception ex)
            {
                failures[boardId] = ex;
            }

            public int GetCallCount(int boardId)
            {
                return callCounts.TryGetValue(boardId, out var count) ? count : 0;
            }

            public float GetLastScore(int boardId)
            {
                return lastScores.TryGetValue(boardId, out var score) ? score : 0f;
            }

            public UniTask SendAsync(int boardId, float score, CancellationToken ct = default)
            {
                callCounts[boardId] = GetCallCount(boardId) + 1;
                lastScores[boardId] = score;

                if (failures.TryGetValue(boardId, out var ex))
                    return UniTask.FromException(ex);

                return UniTask.CompletedTask;
            }
        }

        private class RecordingScoreBoardSender : IScoreBoardSender
        {
            private readonly List<float> sendOrder;
            private readonly int boardFilter;

            public RecordingScoreBoardSender(List<float> sendOrder, int boardFilter)
            {
                this.sendOrder = sendOrder;
                this.boardFilter = boardFilter;
            }

            public UniTask SendAsync(int boardId, float score, CancellationToken ct = default)
            {
                if (boardId == boardFilter)
                    sendOrder.Add(score);
                return UniTask.CompletedTask;
            }
        }
    }
}
