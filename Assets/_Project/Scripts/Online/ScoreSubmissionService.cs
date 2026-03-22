using System;
using System.Collections.Generic;
using System.Threading;
using Action002.Core.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unityroom.Client;

namespace Action002.Online
{
    public class ScoreSubmissionService : MonoBehaviour
    {
        [SerializeField] private string hmacKey;
        [SerializeField] private ScoreSubmitRequestedEventChannelSO onScoreSubmitRequested;

        private const int ScoreBoardId = 1;
        private const int ComboBoardId = 2;

        private UnityroomClient client;
        private IScoreBoardSender sender;
        private bool submitting;
        private readonly Queue<ScoreSubmitRequest> pendingQueue = new Queue<ScoreSubmitRequest>();

        private void OnEnable()
        {
            if (onScoreSubmitRequested != null)
                onScoreSubmitRequested.OnEventRaised += HandleScoreSubmitRequested;
        }

        private void OnDisable()
        {
            if (onScoreSubmitRequested != null)
                onScoreSubmitRequested.OnEventRaised -= HandleScoreSubmitRequested;
        }

        internal void InjectSender(IScoreBoardSender s) => sender = s;

        internal void SendToTarget(ScoreSubmitRequest request)
        {
            HandleScoreSubmitRequested(request);
        }

        private void HandleScoreSubmitRequested(ScoreSubmitRequest request)
        {
            if (submitting)
            {
                pendingQueue.Enqueue(request);
                return;
            }

            StartSubmit(request);
        }

        private void StartSubmit(ScoreSubmitRequest request)
        {

            if (sender == null)
            {
                try
                {
                    client = new UnityroomClient
                    {
                        HmacKey = hmacKey,
                        MaxRetries = 3,
                        Timeout = TimeSpan.FromSeconds(30),
                    };
                    sender = new UnityroomScoreBoardSender(client);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[{GetType().Name}] Failed to initialize client (hmacKey may be invalid): {ex.Message}");
                    return;
                }
            }

            SubmitAsync(request.Score, request.Combo, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid SubmitAsync(int score, int combo, CancellationToken ct)
        {
            submitting = true;
            try
            {
                if (score > 0)
                {
                    try
                    {
                        await sender.SendAsync(ScoreBoardId, score, ct);
                    }
                    catch (OperationCanceledException) { return; }
                    catch (UnityroomApiException ex)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Score board error: {ex.ErrorCode}/{ex.ErrorType}: {ex.Message}");
                    }
                    catch (TimeoutException ex)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Score board timeout: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Score board unexpected: {ex.Message}");
                    }
                }

                if (combo > 0)
                {
                    try
                    {
                        await sender.SendAsync(ComboBoardId, combo, ct);
                    }
                    catch (OperationCanceledException) { return; }
                    catch (UnityroomApiException ex)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Combo board error: {ex.ErrorCode}/{ex.ErrorType}: {ex.Message}");
                    }
                    catch (TimeoutException ex)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Combo board timeout: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Combo board unexpected: {ex.Message}");
                    }
                }
            }
            finally
            {
                submitting = false;

                if (pendingQueue.Count > 0)
                    StartSubmit(pendingQueue.Dequeue());
            }
        }

        private void OnDestroy()
        {
            client?.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(hmacKey))
                Debug.LogWarning($"[{GetType().Name}] hmacKey is empty on {gameObject.name}.", this);
            if (onScoreSubmitRequested == null)
                Debug.LogWarning($"[{GetType().Name}] onScoreSubmitRequested not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
