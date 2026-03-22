using UnityEngine;
using LitMotion;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class AbsorptionPulseEffect : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Transform playerTransform;

        [Header("Settings")]
        [SerializeField] private float scaleUp = 1.15f;
        [SerializeField] private float halfDuration = 0.05f;

        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onComboIncremented;

        private MotionHandle pulseHandle;
        private Vector3 originalScale;

        private void Awake()
        {
            CaptureOriginalScale();
        }

        private void OnEnable()
        {
            CaptureOriginalScale();

            if (onComboIncremented == null)
            {
                return;
            }

            onComboIncremented.OnEventRaised += HandleComboIncremented;
        }

        private void OnDisable()
        {
            CancelPulse();

            if (onComboIncremented == null)
            {
                return;
            }

            onComboIncremented.OnEventRaised -= HandleComboIncremented;
        }

        private void HandleComboIncremented(float _)
        {
            if (playerTransform == null)
            {
                return;
            }

            CancelPulse();
            CaptureOriginalScale();
            StartScaleUpPulse();
        }

        private void CancelPulse()
        {
            if (pulseHandle.IsActive())
            {
                pulseHandle.Cancel();

                if (playerTransform != null)
                {
                    playerTransform.localScale = originalScale;
                }
            }
        }

        private void StartScaleUpPulse()
        {
            pulseHandle = LMotion.Create(1f, scaleUp, halfDuration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(StartScaleDownPulse)
                .Bind(ApplyScaleMultiplier);
        }

        private void StartScaleDownPulse()
        {
            pulseHandle = LMotion.Create(scaleUp, 1f, halfDuration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(ResetScale)
                .Bind(ApplyScaleMultiplier);
        }

        private void ApplyScaleMultiplier(float scaleMultiplier)
        {
            if (playerTransform == null)
            {
                return;
            }

            playerTransform.localScale = originalScale * scaleMultiplier;
        }

        private void ResetScale()
        {
            if (playerTransform == null)
            {
                return;
            }

            playerTransform.localScale = originalScale;
        }

        private void CaptureOriginalScale()
        {
            if (playerTransform == null)
            {
                return;
            }

            originalScale = playerTransform.localScale;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (playerTransform == null)
            {
                Debug.LogWarning($"[{GetType().Name}] playerTransform not assigned on {gameObject.name}.", this);
            }

            if (onComboIncremented == null)
            {
                Debug.LogWarning($"[{GetType().Name}] onComboIncremented not assigned on {gameObject.name}.", this);
            }
        }
#endif
    }
}
